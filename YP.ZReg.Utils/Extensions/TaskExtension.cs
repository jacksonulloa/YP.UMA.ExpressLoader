using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using YP.ZReg.Entities.Generic;
using YP.ZReg.Utils.Helpers;
using YP.ZReg.Utils.Interfaces;

namespace YP.ZReg.Utils.Extensions
{
    public static class TaskExtension
    {
        public static void FireAndForget(this Task task, ILogger logger, string? context = null)
        {
            task.ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    logger.LogError(t.Exception,
                        "Error en tarea FireAndForget{Context}",
                        string.IsNullOrEmpty(context) ? "" : $" ({context})");
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
        public static void FireAndForget(this Task task)
        {
            task.ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    // Manejo interno, log, etc.
                    //Console.WriteLine($"Error en log async: {t.Exception}");
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
        //EJEMPLO DE USO
        /*
         dps.bls.RegistrarLogAsync<TRequest, TResponse>(
            record,
            request,
            response,
            statusCode,
            proceso
        ).FireAndForget(ex => 
        {
            // Manejo de excepción de log
            ToolHelper.LogInternalError("Error al registrar log async", ex);
        });
         */
        public static void FireAndForget(this Task? task, Action<Exception>? onError = null)
        {
            if (task == null) return;

            task.ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    // Usa tu logger centralizado
                    onError?.Invoke(t.Exception);
                    // o en tu caso, podrías hacer:
                    // ToolHelper.LogInternalError("Error en FireAndForget", t.Exception);
                }
            }, TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);
        }
        public static async Task<HttpResponseData> ProcesarResultadoAsync<TRequest, TResponse>(
            IDependencyProviderService dps,
            HttpRequestData req,
            TRequest request,
            TResponse response,
            string proceso,
            DateTime inicio,
            string empresa,
            string nivel,
            HttpStatusCode statusCode)
        {
            DateTime fin = ToolHelper.GetActualPeruHour();

            var record = dps.mpr.Map<BlobTableRecord>(response);
            record.FechaHoraInicio = inicio;
            record.FechaHoraFin = fin;
            record.Proceso = proceso;
            record.Empresa = empresa;
            record.Nivel = nivel;

            dps.bls.RegistrarLogAsync<TRequest, TResponse>(
                record,
                request,
                response,
                statusCode).FireAndForget();
            return await req.ToJsonResponse(response, statusCode);
        }
        public static async Task ProcesarResultadoAsync<TRequest, TResponse>(
            IDependencyProviderService dps,
            TRequest request,
            TResponse response,
            string proceso,
            DateTime inicio,
            string empresa,
            string nivel,
            string codResp,
            string desResp,
            HttpStatusCode statusCode)
        {
            DateTime fin = ToolHelper.GetActualPeruHour();
            BlobTableRecord record = new()
            {
                Empresa = empresa,
                Proceso = proceso,
                FechaHoraInicio = inicio,
                FechaHoraFin = fin,
                Nivel = nivel,
                CodResp = codResp,
                DescResp = desResp
            };
            dps.bls.RegistrarLogAsync<TRequest, TResponse>(
                record,
                request,
                response,
                statusCode).FireAndForget();
        }
        public static async Task<HttpResponseData?> ValidarTokenAsync<TRequest, TResponse>(
            IDependencyProviderService dps,
            HttpRequestData httpReq,
            TRequest requestApi,
            DateTime inicio,
            string proceso,
            string empresa,
            HttpStatusCode statusCode) where TResponse : BaseResponse, new()
        {
            var tokenCheckResult = JwtHelper.ConfirmarTokenObjeto<TRequest, TResponse>(
                httpReq, dps.jwc.SecretKey, dps.jwc.Claim);

            if (tokenCheckResult?.CodResp == "99")
            {
                return await TaskExtension.ProcesarResultadoAsync<TRequest, TResponse>(
                    dps,
                    httpReq,
                    requestApi,
                    tokenCheckResult,
                    $"Fallo Autenticacion {proceso}",
                    inicio,
                    empresa,
                    "Error",
                    statusCode); 
            }
            return null;
        }
        
    }
}
