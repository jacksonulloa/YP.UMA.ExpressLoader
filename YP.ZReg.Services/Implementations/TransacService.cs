using Azure.Core.GeoJson;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Text.Json;
using YP.ZReg.Dtos.Models;
using YP.ZReg.Entities.Generic;
using YP.ZReg.Entities.Model;
using YP.ZReg.Repositories.Implementations;
using YP.ZReg.Repositories.Interfaces;
using YP.ZReg.Services.Interfaces;
using YP.ZReg.Utils.Helpers;
using YP.ZReg.Utils.Interfaces;
using static System.Net.WebRequestMethods;

namespace YP.ZReg.Services.Implementations
{
    public class TransacService(IDependencyProviderService _dps, IAzureSftp _ass, IDeudaRepository _dre,
        IEmpresaCache _eca) : ITransacService
    {
        private readonly IDependencyProviderService dps = _dps;
        private readonly IAzureSftp ass = _ass;
        private readonly IDeudaRepository dre = _dre;
        private readonly IEmpresaCache eca = _eca;

        private const int MinLineLength = 211;
        private const int HeaderPosition = 21;
        private const string ActionReplace = "R";
        private const string ActionNew = "N";
        private const string RecordTypeNew = "N";
        private const string RecordTypeUpdate = "A";

        #region SETTERS
        private static BaseResponseExtension CreateSuccessResponse()
        {
            return new BaseResponseExtension
            {
                CodResp = "00",
                DesResp = "Ok",
                Resume = "Ok",
                StartExec = DateTime.Now
            };
        }

        private static ResumeProcess CreateInitialResumeProcess()
        {
            return new ResumeProcess
            {
                StartExec = DateTime.Now,
                FileName = "NoApply",
                FileType = "NoApply"
            };
        }

        private static ResumeProcess CreateFileResumeProcess(string fileName)
        {
            return new ResumeProcess
            {
                StartExec = DateTime.Now,
                FileName = fileName,
                FileType = "Unknown"
            };
        }

        private static EmpresaPaths CreateEmpresaPaths(string pathCore, string empresaCodigo)
        {
            string root = $"{pathCore}/{empresaCodigo}/Deudas";
            return new EmpresaPaths
            {                
                Root = root,
                Pending = $"{root}/Pending",
                Complete = $"{root}/Complete",
                Error = $"{root}/Error"
            };
        }
        private RowTextRecord ParseLineToRecord(string linea, string empresaCodigo)
        {
            return new RowTextRecord
            {
                CodigoEmpresa = empresaCodigo,
                TipoRegistro = linea[..1].Trim(),
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
        }
        private static string GetFileType(string accion) => accion switch
        {
            ActionReplace => "Replace",
            ActionNew => "News",
            _ => "Novedades"
        };
        private Deuda MapToDeuda(RowTextRecord record)
        {
            var deuda = dps.mpr.Map<Deuda>(record);
            deuda.estado = "P";
            return deuda;
        }
        #endregion
        #region methods
        private List<string> ValidateRecord(RowTextRecord record, string accion, EmpresaCache _empresaCache)
        {
            var errors = new List<string>();
            if (!TxtProcessor.ValidarFecha(record.FechaVencimiento))
                errors.Add("Fecha de vencimiento inválida");
            if (!TxtProcessor.ValidarFecha(record.FechaEmision))
                errors.Add("Fecha de emisión inválida");
            if (!TxtProcessor.ValidarNombre(record.NombreCliente, out string nombreNormalizado))
                errors.Add("Nombre de cliente inválido");
            else
                record.NombreCliente = nombreNormalizado;
            if (!TxtProcessor.ValidarLlave(record.Llave1, true))
                errors.Add("La llave 1 es inválida");
            if (!TxtProcessor.ValidarLlave(record.Dni, false))
                errors.Add("La llave 2 es inválida");
            if (!TxtProcessor.ValidarLlave(record.Ruc, false))
                errors.Add("La llave 3 es inválida");
            if (!TxtProcessor.ValidarLlave(record.Llave4, false))
                errors.Add("La llave 4 es inválida");
            var montos = $"{record.ImporteBruto}{record.Mora}{record.GastoAdministrativo}{record.ImporteMinimo}";
            if (!TxtProcessor.ValidarReglas(montos))
                errors.Add("Un monto es inválido");
            if (!TxtProcessor.ValidarTipoRegistro(accion, record.TipoRegistro))
                errors.Add("El tipo de registro no aplica en archivo de reemplazo");
            // Validaciones de empresa y servicio
            var empresa = _empresaCache.empresas.FirstOrDefault(x => x.id_proveedor == record.CodigoEmpresa);
            if (empresa == null)
            {
                errors.Add("Empresa no encontrada");
                return errors;
            }
            if (!empresa.servicios.Any(x => x.codigo == record.CodigoServicio))
            {
                errors.Add("El servicio no se encuentra registrado");
            }
            else if (!empresa.servicios.Any(x => x.codigo == record.CodigoServicio &&
                                                  x.moneda.ToString().Equals(record.Moneda)))
            {
                errors.Add("Moneda inválida para el servicio");
            }
            return errors;
        }
        private static void AddError(ResumeProcess result, string linea, string error)
        {
            AddError(result, linea, [error]);
        }
        private static void AddError(ResumeProcess result, string linea, List<string> errors)
        {
            result.ErrorDetails.Add(new ResumeErrorRecord
            {
                Row = linea,
                Results = errors
            });
        }
        private async Task ProcessDeuda(Deuda deuda, string accion, string tipoRegistro)
        {
            switch (accion)
            {
                case ActionReplace:
                    await dre.InsertarDeuda(deuda);
                    break;
                default:
                    await ProcessNovelty(deuda, tipoRegistro);
                    break;
            }
        }
        private async Task ProcessNovelty(Deuda deuda, string tipoRegistro)
        {
            switch (tipoRegistro)
            {
                case RecordTypeNew:
                    await dre.InsertarDeuda(deuda);
                    break;
                case RecordTypeUpdate:
                    await dre.ActualizarDeudaEnCarga(deuda);
                    break;
                default:
                    await dre.EliminarDeudaPorHash(deuda);
                    break;
            }
        }
        private static string ExtractAction(string headerLine) => headerLine.Substring(HeaderPosition, 1);
        public async Task<ResumeProcess> ProcessLines(List<string> lineas, string empresaCodigo, EmpresaCache EmpresaCache)
        {
            var result = new ResumeProcess { TotalRecords = (lineas.Count - 1).ToString() };
            var accion = ExtractAction(lineas[0]);
            result.FileType = GetFileType(accion);

            if (accion == ActionReplace)
            {
                await dre.EliminarDeudaPorEmpresa(empresaCodigo);
            }

            var processingTasks = lineas.Skip(1)
                .Select((linea, index) => ProcessLine(linea, index + 1, accion, empresaCodigo, result, EmpresaCache));

            await Task.WhenAll(processingTasks);
            return result;
        }
        private async Task ProcessLine(string linea, int lineNumber, string accion, string empresaCodigo, ResumeProcess result, EmpresaCache empresasCache)
        {
            if (linea.Trim().Length < MinLineLength)
            {
                AddError(result, linea, "Longitud de fila inválida");
                return;
            }

            var record = ParseLineToRecord(linea, empresaCodigo);
            var validationErrors = ValidateRecord(record, accion, empresasCache);

            if (validationErrors.Any())
            {
                AddError(result, linea, validationErrors);
                return;
            }

            try
            {
                var deuda = MapToDeuda(record);
                await ProcessDeuda(deuda, accion, record.TipoRegistro);
            }
            catch (SqlException ex) when (ex.Number is 2627 or 2601)
            {
                AddError(result, linea, "Registro duplicado");
            }
            catch (SqlException ex) when (ex.Number == 50002)
            {
                AddError(result, linea, "El estado del registro no está como pendiente");
            }
            catch (Exception ex)
            {
                AddError(result, linea, $"Error: {ex.Message}");
            }
        }
        private bool IsValidEmpresa(string empresaCodigo, EmpresaCache empresasCache)
        {
            return empresasCache.empresas?.Any(x => x.id_proveedor.Equals(empresaCodigo)) == true;
        }

        private async Task HandleInvalidEmpresa(EmpresaConfig empresa, string completePath, ResumeProcess resumeProcess)
        {
            resumeProcess.EndExec = DateTime.Now;
            resumeProcess.ErrorRecords = "0";
            resumeProcess.TotalRecords = "0";

            var errorRecord = new ResumeErrorRecord
            {
                Row = "Generic",
                Results = ["El código de la carpeta no se asocia a una empresa dentro de la base de datos"]
            };

            resumeProcess.ErrorDetails = new List<ResumeErrorRecord> { errorRecord };
            resumeProcess.SuccessRecords = "0";
            resumeProcess.Duration = ToolHelper.CalcularDuracion(resumeProcess.StartExec, resumeProcess.EndExec);

            await SaveProcessingResult(completePath, resumeProcess, $"ERROR:{empresa.Codigo}");
        }

        private static void UpdateResumeProcessWithResults(ResumeProcess resumeProcess, ResumeProcess result)
        {
            resumeProcess.EndExec = DateTime.Now;
            resumeProcess.TotalRecords = result.TotalRecords.ToString();
            resumeProcess.ErrorRecords = result.ErrorRecords;
            resumeProcess.SuccessRecords = (int.Parse(resumeProcess.TotalRecords) - int.Parse(resumeProcess.ErrorRecords)).ToString();
            resumeProcess.ErrorDetails = result.ErrorDetails;
            resumeProcess.FileType = result.FileType;
            resumeProcess.Duration = ToolHelper.CalcularDuracion(resumeProcess.StartExec, resumeProcess.EndExec);
        }
        private async Task MoveProcessedFile(string sourceFile, string targetPath, ResumeProcess resumeProcess)
        {
            var fileName = Path.GetFileNameWithoutExtension(resumeProcess.FileName);
            var timestamp = DateTime.Now.ToString("ddMMyyyyHHmmssfff");
            var newFileName = $"{fileName}_{timestamp}";

            await SaveProcessingResult(targetPath, resumeProcess, newFileName);
            await ass.MoveFileAsync(sourceFile, $"{targetPath}/input_{newFileName}.TXT");
        }

        private async Task SaveProcessingResult(string targetPath, ResumeProcess resumeProcess, string fileName)
        {
            var json = JsonSerializer.Serialize(resumeProcess, new JsonSerializerOptions { WriteIndented = true });
            await ass.UploadJsonAsync($"{targetPath}/resume_{fileName}.json", json, Encoding.UTF8, default);
        }

        //private record EmpresaPaths(string pending, string complete, string error);
        #endregion




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
                                    //BaseResponse error = new();
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
                                            //error = new();
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
