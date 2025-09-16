using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using YP.ZReg.Dtos.Models;
using YP.ZReg.Entities.Generic;
using YP.ZReg.Entities.Model;
using YP.ZReg.Repositories.Interfaces;
using YP.ZReg.Services.Interfaces;
using YP.ZReg.Utils.Helpers;
using YP.ZReg.Utils.Interfaces;

namespace YP.ZReg.Services.Implementations
{
    public class GeneratorService(IDependencyProviderService _dps, IAzureSftp _ass, ITransaccionRepository _trr,
        IEmpresaCache _eca) : IGeneratorService
    {
        private readonly IDependencyProviderService dps = _dps;
        private readonly IAzureSftp ass = _ass;
        private readonly ITransaccionRepository trr = _trr;
        private readonly IEmpresaCache eca = _eca;

        public async Task<BaseResponseExtension> WriteFilesAsync()
        {
            BaseResponseExtension response = new() { CodResp = "00", DesResp = "Ok", Resume = "Ok", StartExec = DateTime.Now };
            ConcurrentBag<ResumeGeneratorProcess> empresasConError = [];
            try
            {
                if (dps.cnf.EmpresasConfig.Count == 0)
                {
                    ToolHelper.SetResponse(response, "22", "No se encontraron configuraciones de empresas");
                    ToolHelper.SetFinalResponse(response);
                    return response;
                }
                await Task.WhenAll(dps.cnf.EmpresasConfig.Select(async empresa =>
                {
                    try
                    {
                        ResumeGeneratorProcess procesamiento = await ProcesarEmpresaAsync(empresa);
                        if(procesamiento != null) empresasConError.Add(procesamiento);
                    }
                    catch (Exception ex)
                    {
                        //empresasConError.Add(empresa.Codigo);
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
        private async Task<ResumeGeneratorProcess?> ProcesarEmpresaAsync(EmpresaConfig empresa)
        {
            ResumeGeneratorProcess? resumen = new() { idEmpresa = empresa.Codigo, description = "Ok" };
            List<ResumeLoadErrorRecord> listaErrores = [];
            List<Transaccion> transacciones = await trr.ConsultarDeuda(empresa.Codigo, "P");
            if (transacciones.Count == 0) return null;
            EmpresaPaths paths = ToolHelper.CreateEmpresaPaths(dps.sft.Root, empresa.Codigo);
            string fileName = $"{empresa.Codigo}_{DateTime.Now:ddMMyyyyhhmmssfff}.txt";
            string resumeFileName = $"{empresa.Codigo}_{DateTime.Now:ddMMyyyyhhmmssfff}.json";
            string jsonResumen = "";
            try
            {
                await ass.UploadJsonAsync($"{paths.PagosRoot}/{fileName}", "", Encoding.UTF8, default);
            }
            catch (Exception ex)
            {
                resumen = new() { idEmpresa = empresa.Codigo, errorRecordIds = transacciones.Select(x => x.id).ToList(), okRecordIds = [], description = $"Error => {ex.Message}" };
                jsonResumen = JsonSerializer.Serialize(resumen, new JsonSerializerOptions { WriteIndented = true });
                await ass.UploadJsonAsync($"{paths.PagosRoot}/{resumeFileName}", jsonResumen, Encoding.UTF8, default);
                return resumen;
            }
            (resumen.okRecordIds, resumen.errorRecordIds) = await ass.WriteAllLinesAsync($"{paths.PagosRoot}/{fileName}", transacciones, Encoding.UTF8, default);
            jsonResumen = JsonSerializer.Serialize(resumen, new JsonSerializerOptions { WriteIndented = true });
            await ass.UploadJsonAsync($"{paths.PagosRoot}/{resumeFileName}", jsonResumen, Encoding.UTF8, default);
            if (resumen.okRecordIds.Count > 0) await trr.ActualizarEstadoTransacciones(empresa.Codigo, string.Join(',', resumen.okRecordIds), "C");            
            return resumen;
        }
    }
}
