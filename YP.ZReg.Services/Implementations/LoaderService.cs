using Microsoft.Data.SqlClient;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using YP.ZReg.Dtos.Contracts.Request;
using YP.ZReg.Dtos.Contracts.Response;
using YP.ZReg.Dtos.Models;
using YP.ZReg.Entities.Generic;
using YP.ZReg.Entities.Model;
using YP.ZReg.Repositories.Interfaces;
using YP.ZReg.Services.Interfaces;
using YP.ZReg.Utils.Extensions;
using YP.ZReg.Utils.Helpers;
using YP.ZReg.Utils.Interfaces;

namespace YP.ZReg.Services.Implementations
{
    public class LoaderService(IDependencyProviderService _dps, IAzureSftp _ass, IDeudaRepository _dre,
        IEmpresaCache _eca) : ILoaderService
    {
        private readonly IDependencyProviderService dps = _dps;
        private readonly IAzureSftp ass = _ass;
        private readonly IDeudaRepository dre = _dre;
        private readonly IEmpresaCache eca = _eca;
        public async Task<BaseResponseExtension> ReadFilesAsync()
        {
            BaseResponseExtension response = new() { CodResp = "00", DesResp = "Ok", Resume = "Ok", StartExec = DateTime.Now };
            ConcurrentBag<string> empresasConError = [];
            try
            {
                if (dps.cnf.EmpresasConfig.Count == 0)
                {
                    ToolHelper.SetResponse(response, "22", "No se encontraron configuraciones de empresas");
                    ToolHelper.SetFinalResponse(response);
                    return response;
                }
                //var tasksEmpresas = dps.cnf.EmpresasConfig.Select(async empresa =>
                //{
                //    await ProcesarEmpresaAsync(empresa);
                //});
                //await Task.WhenAll(tasksEmpresas);
                await Task.WhenAll(dps.cnf.EmpresasConfig.Select(async empresa =>
                {
                    try
                    {
                        await ProcesarEmpresaAsync(empresa);
                    }
                    catch (Exception ex)
                    {
                        empresasConError.Add(empresa.Codigo);
                    }
                }));
            }
            catch (Exception ex)
            {
                ToolHelper.SetErrorResponse(response, ex);
            }
            finally
            {
                string resumenError = string.Join(",", empresasConError);
                if (!string.IsNullOrWhiteSpace(resumenError)) ToolHelper.SetResponse(response, "00", $"Errores en las empresas: {resumenError}");
                ToolHelper.SetFinalResponse(response);
            }
            return response;
        }
        private async Task ProcesarEmpresaAsync(EmpresaConfig empresa)
        {
            ResumeLoadProcess resumeProcess = SetInitialResumeProcess("NoApply", "NoApply");
            //ResumeErrorRecord resumeErrorRecord = new();
            List<ResumeLoadErrorRecord> listaErrores = [];
            EmpresaPaths paths = ToolHelper.CreateEmpresaPaths(dps.sft.Root, empresa.Codigo);
            var empresasFounded = eca.empresas.Where(x => x.id_proveedor.Equals(empresa.Codigo)).ToList();
            if (empresasFounded.Count == 0)
            {
                listaErrores.Add(new ResumeLoadErrorRecord
                {
                    Row = "Generic",
                    Results = ["El codigo de la carpeta no se asocia a una empresa dentro de la base de datos"]
                });
                SetFinalResumeProcess(resumeProcess, "0", "0", listaErrores);
                string fileNewName = $"ERROR_{empresa.Codigo}_{resumeProcess.EndExec:ddMMyyyyhhmmssfff}";
                string json = JsonSerializer.Serialize(resumeProcess, new JsonSerializerOptions { WriteIndented = true });
                string archivoFinal = $"{paths.DeudasComplete}/{fileNewName}";
                await ass.UploadJsonAsync($"{paths.DeudasComplete}/resume_{fileNewName}.json", json, Encoding.UTF8, default);

                TaskExtension.ProcesarResultadoAsync<object, ResumeCompactLoadProcess>(dps,
                    null, resumeProcess, "Carga", resumeProcess.StartExec, empresa.Codigo, "Info",
                    "22", "Ok", HttpStatusCode.Accepted).FireAndForget();

                return;
            }
            var archivos = await ass.ListAsync(paths.DeudasPending);
            if (archivos.Count > 0)
            {
                foreach (string archivo in archivos)
                {
                    resumeProcess = SetInitialResumeProcess(archivo, "PreLoad");
                    var lineas = await ass.ReadAllLinesAsync(archivo, Encoding.UTF8, default);
                    resumeProcess.TotalRecords = lineas is null ? "0" : (lineas.Count - 1).ToString();
                    if (lineas is not null && lineas.Count > 1)
                    {
                        string accion = string.Empty;
                        int contador = 0;
                        foreach (string linea in lineas)
                        {
                            if (contador == 0)
                            {
                                accion = linea.Substring(21, 1);
                                resumeProcess.FileType = accion.Equals("R") ? "Replace" : (accion.Equals("N") ? "News" : "Unkown");
                                Func<Task> accionEliminar = accion.Equals("R")
                                                                ? () => dre.EliminarDeudaPorEmpresa(empresa.Codigo)
                                                                : () => Task.CompletedTask;

                                await accionEliminar();
                                contador++;
                                continue;
                            }
                            (ResumeLoadErrorRecord resumeErrorRecord, RowTextRecord? record) = ValidateRecord(empresa.Codigo, linea, accion);
                            if (resumeErrorRecord.Results.Count > 0) { listaErrores.Add(resumeErrorRecord); continue; }
                            Deuda deuda = dps.mpr.Map<Deuda>(record);
                            deuda.estado = "P";
                            try
                            {
                                if (record.TipoRegistro.Equals("N"))
                                {
                                    await dre.InsertarDeuda(deuda);
                                }
                                else if (record.TipoRegistro.Equals("A"))
                                {
                                    await dre.ActualizarDeudaEnCarga(deuda);
                                }
                                else if (record.TipoRegistro.Equals("D"))
                                {
                                    await dre.EliminarDeudaPorHash(deuda);
                                }
                                else
                                {
                                    resumeErrorRecord.Results.Add($"Tipo de registro invalido");
                                    listaErrores.Add(resumeErrorRecord);
                                }
                            }
                            catch (SqlException ex) when (ex.Number is 2627 or 2601)
                            {
                                resumeErrorRecord.Results.Add($"Registro duplicado");
                                listaErrores.Add(resumeErrorRecord);
                            }
                            catch (SqlException ex) when (ex.Number == 50002)
                            {
                                resumeErrorRecord.Results.Add("El estado del registro no esta como pendiente");
                                listaErrores.Add(resumeErrorRecord);
                            }
                            catch (SqlException ex)
                            {
                                resumeErrorRecord.Results.Add($"Error SQL => {ex.Message}");
                                listaErrores.Add(resumeErrorRecord);
                            }
                            catch (Exception exc)
                            {
                                resumeErrorRecord.Results.Add($"Error => {exc.Message}");
                                listaErrores.Add(resumeErrorRecord);
                            }
                        }
                    }
                    SetFinalResumeProcess(resumeProcess, resumeProcess.TotalRecords, listaErrores.Count.ToString(), listaErrores);
                    string fileSinExtension = Path.GetFileNameWithoutExtension(resumeProcess.FileName);
                    DateTime FechaHoraExec = ToolHelper.GetActualPeruHour();
                    string fileNewName = $"{fileSinExtension}_{FechaHoraExec:ddMMyyyyhhmmssfff}";
                    string json = JsonSerializer.Serialize(resumeProcess, new JsonSerializerOptions { WriteIndented = true });
                    //string archivoFinal = $"{paths.Complete}/{fileNewName}";
                    await ass.UploadJsonAsync($"{paths.DeudasComplete}/resume_{fileNewName}.json", json, Encoding.UTF8, default);
                    await ass.MoveFileAsync(archivo, $"{paths.DeudasComplete}/input_{fileNewName}.TXT");
                }
                ResumeCompactLoadProcess resumeCompactLoadProcess = dps.mpr.Map<ResumeCompactLoadProcess>(resumeProcess);
                TaskExtension.ProcesarResultadoAsync<object, ResumeCompactLoadProcess>(dps,
                    null, resumeCompactLoadProcess, "Carga", resumeCompactLoadProcess.StartExec, empresa.Codigo, "Info",
                    "00", "Ok", HttpStatusCode.Accepted).FireAndForget();
            }            
        }
        private (ResumeLoadErrorRecord, RowTextRecord?) ValidateRecord(string codigoEmpresa, string linea, string accion)
        {
            ResumeLoadErrorRecord resume = new() { Row = linea };
            RowTextRecord? record = null;
            var errors = new List<string>();
            try
            {
                if (linea.Length < 211)
                {
                    errors.Add("Longitud de fila invalida");
                    return (resume, record);
                }
                record = new RowTextRecord
                {
                    CodigoEmpresa = codigoEmpresa,
                    TipoRegistro = linea.Substring(0, 1).Trim(),
                    Llave1 = linea.Substring(1, 14).Trim(),
                    NombreCliente = linea.Substring(15, 30).Trim(),
                    CodigoServicio = linea.Substring(45, 3).Trim(),
                    NroDocumento = linea.Substring(48, 16).Trim(),
                    Glosa = linea.Substring(64, 20).Trim(),
                    FechaVencimiento = linea.Substring(84, 8).Trim(),
                    FechaEmision = linea.Substring(92, 8).Trim(),
                    ImporteBruto = linea.Substring(100, 12),
                    Mora = linea.Substring(112, 12),
                    GastoAdministrativo = linea.Substring(124, 12),
                    ImporteMinimo = linea.Substring(136, 12),
                    Periodo = linea.Substring(148, 2).Trim(),
                    Anio = linea.Substring(150, 4).Trim(),
                    Cuota = linea.Substring(154, 2).Trim(),
                    Moneda = linea.Substring(156, 1).Trim(),
                    Dni = linea.Substring(157, 8).Trim(),
                    Ruc = linea.Substring(165, 11).Trim(),
                    Llave4 = linea.Substring(176, 14).Trim(),
                    Filler = linea.Substring(190, 20).Trim(),
                    Cierre = linea.Length > 210 ? linea.Substring(210).Trim() : string.Empty
                };
                if (!TxtProcessor.ValidarFecha(record.FechaVencimiento))
                    errors.Add("Fecha de vencimiento invalida");
                if (!TxtProcessor.ValidarFecha(record.FechaEmision))
                    errors.Add("Fecha de emisión invalida");
                if (!TxtProcessor.ValidarNombre(record.NombreCliente, out string nombreNormalizado))
                    errors.Add("Nombre de cliente invalido");
                else
                    record.NombreCliente = nombreNormalizado;
                if (!TxtProcessor.ValidarLlave(record.Llave1, true))
                    errors.Add("La llave 1 es invalida");
                if (!TxtProcessor.ValidarLlave(record.Dni, false))
                    errors.Add("La llave 2 es invalida");
                if (!TxtProcessor.ValidarLlave(record.Ruc, false))
                    errors.Add("La llave 3 es invalida");
                if (!TxtProcessor.ValidarLlave(record.Llave4, false))
                    errors.Add("La llave 4 es invalida");
                var montos = $"{record.ImporteBruto}{record.Mora}{record.GastoAdministrativo}{record.ImporteMinimo}";
                if (!TxtProcessor.ValidarReglas(montos))
                    errors.Add("Un monto es invalido");
                if (!TxtProcessor.ValidarTipoRegistro(accion, record.TipoRegistro))
                    errors.Add("El tipo de registro no aplica en archivo de reemplazo");
                // Validaciones de empresa y servicio
                var empresa = eca.empresas.FirstOrDefault(x => x.id_proveedor == record.CodigoEmpresa);
                if (empresa == null)
                {
                    errors.Add("Empresa no encontrada");
                }
                else
                {
                    if (!empresa.servicios.Any(x => x.codigo == record.CodigoServicio))
                    {
                        errors.Add("El servicio no se encuentra registrado");
                    }
                    else if (!empresa.servicios.Any(x => x.codigo == record.CodigoServicio &&
                                                          x.moneda.ToString().Equals(record.Moneda)))
                    {
                        errors.Add("Moneda invalida para el servicio");
                    }
                }
            }
            catch (Exception exc)
            {
                errors.Add($"Error=>{exc.Message}");
            }
            finally
            {
                resume.Results = errors;
            }
            return (resume, record);
        }
        private ResumeLoadProcess SetInitialResumeProcess(string fileName, string fileType) => new()
        {
            FileName = fileName,
            FileType = fileType,
            TotalRecords = "0",
            ErrorRecords = "0",
            StartExec = ToolHelper.GetActualPeruHour(),
            EndExec = ToolHelper.GetActualPeruHour(),
            ErrorDetails = []
        };
        private static void SetFinalResumeProcess(ResumeLoadProcess resumeProcess, string total, string errors, List<ResumeLoadErrorRecord> errorsDetail)
        {
            resumeProcess.EndExec = ToolHelper.GetActualPeruHour();
            resumeProcess.TotalRecords = total;
            resumeProcess.ErrorRecords = errors;
            resumeProcess.SuccessRecords = (int.Parse(resumeProcess.TotalRecords) - int.Parse(resumeProcess.ErrorRecords)).ToString();
            resumeProcess.ErrorDetails = errorsDetail;
            resumeProcess.Duration = ToolHelper.CalcularDuracion(resumeProcess.StartExec, resumeProcess.EndExec);
        }
    }
}
