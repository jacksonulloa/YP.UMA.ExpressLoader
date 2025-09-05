using Azure.Core.GeoJson;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Text.Json;
using YP.ZReg.Dtos.Models;
using YP.ZReg.Entities.Generic;
using YP.ZReg.Entities.Model;
using YP.ZReg.Repositories.Interfaces;
using YP.ZReg.Services.Interfaces;
using YP.ZReg.Utils.Helpers;
using YP.ZReg.Utils.Interfaces;
using static System.Net.WebRequestMethods;

namespace YP.ZReg.Services.Implementations
{
    public class TransacService(IDependencyProviderService _dps, IAzureSftp _ass, IDeudaRepository _dre,
        EmpresaCache _eca) : ITransacService
    {
        private readonly IDependencyProviderService dps = _dps;
        private readonly IAzureSftp ass = _ass;
        private readonly IDeudaRepository dre = _dre;
        private readonly EmpresaCache eca = _eca;
        public async Task<BaseResponseExtension> ReadFiles()
        {
            BaseResponseExtension response = new() { CodResp = "00", DesResp = "Ok", Resume = "Ok", StartExec = DateTime.Now };
            List<ResumeErrorRecord> listaErrores = [];
            List<RowTextRecord> listaValidos = [];
            ResumeProcess resumeProcess = new();
            try
            {
                string PathCore = dps.sft.Root;
                if (dps.cnf.EmpresasConfig.Count > 0)
                {
                    //Si la empresa existe debe validarse aqui abajo
                    foreach (var empresa in dps.cnf.EmpresasConfig)
                    {
                        string pendingpath = $"{PathCore}/{empresa.Codigo}/Deudas/Pending";
                        string completepath = $"{PathCore}/{empresa.Codigo}/Deudas/Complete";
                        string errorpath = $"{PathCore}/{empresa.Codigo}/Deudas/Error";
                        resumeProcess = new() {
                            StartExec = DateTime.Now,
                            FileName = "NoApply",
                            FileType = "NoApply"
                        };
                        if (eca.empresas.Where(x => x.id_proveedor.Equals(empresa.Codigo)).ToList().Count > 0)
                        {
                            var archivos = await ass.ListAsync(pendingpath);
                            if (archivos.Count > 0)
                            {
                                foreach (string archivo in archivos)
                                {
                                    resumeProcess = new()
                                    {
                                        StartExec = DateTime.Now,
                                        FileName = Path.GetFileName(archivo),
                                        FileType = "Unkown"
                                    };
                                    ResumeErrorRecord resume = new();
                                    BaseResponse error = new();
                                    var lineas = await ass.ReadAllLinesAsync(archivo, Encoding.UTF8, default);
                                    resumeProcess.TotalRecords = lineas is null ? "0" : (lineas.Count - 1).ToString();
                                    if (lineas is not null && lineas.Count > 1)
                                    {
                                        listaErrores = [];
                                        string accion = string.Empty;
                                        int contador = 0;
                                        foreach (string linea in lineas)
                                        {
                                            resume = new()
                                            {
                                                Results = [],
                                                Row = linea
                                            };
                                            error = new();
                                            if (contador == 0)
                                            {
                                                accion = linea.Substring(21, 1);
                                                if (accion.Equals("R"))
                                                {
                                                    resumeProcess.FileType = "Replace";
                                                    await dre.EliminarDeudaPorEmpresa(empresa.Codigo);
                                                }
                                                else
                                                {
                                                    resumeProcess.FileType = accion.Equals("N") ? "News" : "Novedades";
                                                }
                                            }
                                            else
                                            {
                                                if (linea.Trim().Length >= 211)
                                                {
                                                    var record = new RowTextRecord
                                                    {
                                                        CodigoEmpresa = empresa.Codigo,
                                                        TipoRegistro = linea.Substring(0, 1).Trim(),
                                                        Llave1 = linea.Substring(1, 14).Trim(),
                                                        NombreCliente = linea.Substring(15, 30).Trim(),
                                                        CodigoServicio = linea.Substring(45, 3).Trim(),
                                                        NroDocumento = linea.Substring(48, 16).Trim(),
                                                        NomServicio = linea.Substring(64, 20).Trim(),
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
                                                    if (!TxtProcessor.ValidarFecha(record.FechaVencimiento)) resume.Results.Add("Fecha de vencimiento invalida");
                                                    if (!TxtProcessor.ValidarFecha(record.FechaEmision)) resume.Results.Add("Fecha de emision invalida");
                                                    if (!TxtProcessor.ValidarNombre(record.NombreCliente, out string nombreNormalizado))
                                                    {
                                                        resume.Results.Add("Nombre de cliente invalido");
                                                    }
                                                    record.NombreCliente = nombreNormalizado;
                                                    if (!TxtProcessor.ValidarLlave(record.Llave1, true)) resume.Results.Add("La llave 1 es invalida");
                                                    if (!TxtProcessor.ValidarLlave(record.Dni, false)) resume.Results.Add("La llave 2 es invalida");
                                                    if (!TxtProcessor.ValidarLlave(record.Ruc, false)) resume.Results.Add("La llave 3 es invalida");
                                                    if (!TxtProcessor.ValidarLlave(record.Llave4, false)) resume.Results.Add("La llave 4 es invalida");
                                                    if (!TxtProcessor.ValidarReglas($"{record.ImporteBruto}{record.Mora}{record.GastoAdministrativo}{record.ImporteMinimo}")) resume.Results.Add("Un monto es invalido");
                                                    if (!TxtProcessor.ValidarTipoRegistro(accion, record.TipoRegistro))
                                                    {
                                                        resume.Results.Add("El tipo de registro no aplicaen archivo de reemplazo");
                                                    }
                                                    if (resume.Results.Count > 0)
                                                    {
                                                        listaErrores.Add(resume);
                                                    }
                                                    else
                                                    {
                                                        //listaValidos.Add(record);
                                                        Deuda deuda = dps.mpr.Map<Deuda>(record);
                                                        deuda.estado = "P";
                                                        Empresa empresaTemp = eca.empresas.Where(x => x.id_proveedor == deuda.id_empresa).First();
                                                        try
                                                        {
                                                            if (empresaTemp.servicios.Where(x => x.codigo == deuda.servicio).ToList().Count == 0)
                                                            {
                                                                resume.Results.Add("El servicio no se encuentra registrado");
                                                                listaErrores.Add(resume);
                                                            }
                                                            else
                                                            {
                                                                if (empresaTemp.servicios.Where(x => x.codigo == deuda.servicio && x.moneda.ToString().Equals(deuda.moneda)).ToList().Count == 0)
                                                                {
                                                                    resume.Results.Add("Moneda invalida para el servicio");
                                                                    listaErrores.Add(resume);
                                                                }
                                                                else
                                                                {
                                                                    if (accion.Equals("R"))
                                                                    {
                                                                        await dre.InsertarDeuda(deuda);
                                                                    }
                                                                    else
                                                                    {
                                                                        if (record.TipoRegistro.Equals("N"))
                                                                        {
                                                                            await dre.InsertarDeuda(deuda);
                                                                        }
                                                                        else if (record.TipoRegistro.Equals("A"))
                                                                        {
                                                                            await dre.ActualizarDeudaEnCarga(deuda);
                                                                        }
                                                                        else
                                                                        {
                                                                            await dre.EliminarDeudaPorHash(deuda);
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        catch (SqlException ex) when (ex.Number is 2627 or 2601)
                                                        {
                                                            resume.Results.Add($"Registro duplicado");
                                                            listaErrores.Add(resume);
                                                        }
                                                        catch (SqlException ex) when (ex.Number == 50002)
                                                        {
                                                            resume.Results.Add("El estado del registro no esta como pendiente");
                                                            listaErrores.Add(resume);
                                                        }
                                                        catch (SqlException ex)
                                                        {
                                                            resume.Results.Add($"Error SQL => {ex.Message}");
                                                            listaErrores.Add(resume);
                                                        }
                                                        catch (Exception exc)
                                                        {
                                                            resume.Results.Add($"Error => {exc.Message}");
                                                            listaErrores.Add(resume);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    resume.Results.Add("Longitud de fila invalida");
                                                    listaErrores.Add(resume);
                                                }
                                            }
                                            contador++;
                                        }
                                    }
                                    resumeProcess.EndExec = DateTime.Now;                                    
                                    resumeProcess.ErrorRecords = listaErrores.Count.ToString();
                                    resumeProcess.SuccessRecords = (int.Parse(resumeProcess.TotalRecords) - int.Parse(resumeProcess.ErrorRecords)).ToString();
                                    resumeProcess.ErrorDetails = listaErrores;
                                    resumeProcess.Duration = ToolHelper.CalcularDuracion(resumeProcess.StartExec, resumeProcess.EndExec);
                                    string fileSinExtension = Path.GetFileNameWithoutExtension(resumeProcess.FileName);
                                    DateTime FechaHoraExec = DateTime.Now;
                                    string fileNewName = $"{fileSinExtension}_{FechaHoraExec:ddMMyyyyhhmmssfff}";
                                    string json = JsonSerializer.Serialize(resumeProcess, new JsonSerializerOptions { WriteIndented = true });
                                    string archivoFinal = $"{completepath}/{fileNewName}";
                                    await ass.UploadJsonAsync($"{completepath}/resume_{fileNewName}.json", json, Encoding.UTF8, default);
                                    await ass.MoveFileAsync(archivo, $"{completepath}/input_{fileNewName}.TXT");
                                }
                            }
                        }
                        else
                        {
                            resumeProcess.EndExec = DateTime.Now;
                            resumeProcess.ErrorRecords = "0";
                            resumeProcess.TotalRecords = "0";
                            listaErrores = [];
                            ResumeErrorRecord resume = new()
                            {
                                Row = "Generic",
                                Results = ["El codigo de la carpeta no se asocia a una empresa dentro de la base de datos"]
                            };
                            listaErrores.Add(resume);
                            resumeProcess.SuccessRecords = (int.Parse(resumeProcess.TotalRecords) - int.Parse(resumeProcess.ErrorRecords)).ToString();
                            resumeProcess.ErrorDetails = listaErrores;
                            resumeProcess.Duration = ToolHelper.CalcularDuracion(resumeProcess.StartExec, resumeProcess.EndExec);
                            string fileNewName = $"ERROR:{empresa.Codigo}_{resumeProcess.EndExec:ddMMyyyyhhmmssfff}";
                            string json = JsonSerializer.Serialize(resumeProcess, new JsonSerializerOptions { WriteIndented = true });
                            string archivoFinal = $"{completepath}/{fileNewName}";
                            await ass.UploadJsonAsync($"{completepath}/resume_{fileNewName}.json", json, Encoding.UTF8, default);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            return response;
        }
    }
}
