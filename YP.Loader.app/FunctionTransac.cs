using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;
using YP.ZReg.Dtos.Contracts.Request;
using YP.ZReg.Dtos.Contracts.Response;
using YP.ZReg.Services.Interfaces;
using YP.ZReg.Utils.Extensions;
using YP.ZReg.Utils.Helpers;
using YP.ZReg.Utils.Interfaces;

namespace YP.Loader.app
{
    public class FunctionTransac(IDependencyProviderService _dps, IApiSecurityService _ass, IApiTransacService _ats)
    {
        private readonly IApiSecurityService ass = _ass;
        private readonly IApiTransacService ats = _ats;
        private readonly IDependencyProviderService dps = _dps;

        [Function("ObtenerToken")]
        [OpenApiOperation(operationId: "ObtenerToken", tags: new[] { "Seguridad" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(GetTokenReq), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(GetTokenRes), Description = "Token generado")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(GetTokenRes), Description = "Acceso no autorizado")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(GetTokenRes), Description = "Error controlado")]
        public async Task<HttpResponseData> Autenticar([HttpTrigger(AuthorizationLevel.Anonymous, "post",
            Route = "V1.0/seguridad/ObtenerToken")] HttpRequestData req, FunctionContext context)
        {
            GetTokenReq requestApi = await req.ToJsonRequest<GetTokenReq>();
            var (responseApi, statusCode) = await ass.GetTokenWithClaim(requestApi);
            return await req.ToJsonResponse(responseApi, statusCode);
        }

        [Function("Consultar")]
        [OpenApiOperation(operationId: "Consultar", tags: ["DataInfo"])]
        [OpenApiSecurity("Bearer", SecuritySchemeType.ApiKey, In = OpenApiSecurityLocationType.Header, Name = "Authorization")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(GetDebtsReq), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(GetDebtsRes), Description = "Procesamiento correcto")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(GetDebtsRes), Description = "Problemas de autenticacion")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(GetDebtsRes), Description = "Problemas en backend al invocar endpoint")]
        public async Task<HttpResponseData> Consultar(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1.0/transac/consultar")] HttpRequestData httpReq,
        FunctionContext context)
        {
            GetDebtsReq requestApi = await httpReq.ToJsonRequest<GetDebtsReq>();
            DateTime start = ToolHelper.GetActualPeruHour();
            var authResponse = await TaskExtension.ValidarTokenAsync<GetDebtsReq, GetDebtsRes>(dps,
                httpReq, requestApi, start, "Consulta", requestApi.empresa, HttpStatusCode.Accepted);
            if (authResponse != null) return authResponse;
            var (responseApi, statusCode) = await ats.GetDebtsAsync(requestApi);
            return await TaskExtension.ProcesarResultadoAsync<GetDebtsReq, GetDebtsRes>(
                                dps,
                                httpReq,
                                requestApi,
                                responseApi,
                                "Consulta",
                                start,
                                requestApi.empresa,
                                "Info",
                                HttpStatusCode.Accepted);
        }

        [Function("Pagar")]
        [OpenApiOperation(operationId: "Pagar", tags: ["DataInfo"])]
        [OpenApiSecurity("Bearer", SecuritySchemeType.ApiKey, In = OpenApiSecurityLocationType.Header, Name = "Authorization")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(ExecPaymentReq), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ExecPaymentRes), Description = "Procesamiento correcto")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(ExecPaymentRes), Description = "Problemas de autenticacion")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ExecPaymentRes), Description = "Problemas en backend al invocar endpoint")]
        public async Task<HttpResponseData> Pagar(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1.0/transac/pagar")] HttpRequestData httpReq,
        FunctionContext context)
        {
            ExecPaymentReq requestApi = await httpReq.ToJsonRequest<ExecPaymentReq>();
            DateTime start = ToolHelper.GetActualPeruHour();
            var authResponse = await TaskExtension.ValidarTokenAsync<ExecPaymentReq, ExecPaymentRes>(dps,
                httpReq, requestApi, start, "Pago", requestApi.idEmpresa, HttpStatusCode.Accepted);
            if (authResponse != null) return authResponse;
            var (responseApi, statusCode) = await ats.ExecPaymentAsync(requestApi);
            return await TaskExtension.ProcesarResultadoAsync<ExecPaymentReq, ExecPaymentRes>(
                                dps,
                                httpReq,
                                requestApi,
                                responseApi,
                                "Pago",
                                start,
                                requestApi.idEmpresa,
                                "Info",
                                HttpStatusCode.Accepted);
        }

        [Function("Revertir")]
        [OpenApiOperation(operationId: "Revertir", tags: ["DataInfo"])]
        [OpenApiSecurity("Bearer", SecuritySchemeType.ApiKey, In = OpenApiSecurityLocationType.Header, Name = "Authorization")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(ExecReverseReq), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ExecReverseRes), Description = "Procesamiento correcto")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(ExecReverseRes), Description = "Problemas de autenticacion")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ExecReverseRes), Description = "Problemas en backend al invocar endpoint")]
        public async Task<HttpResponseData> Revertir(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "V1.0/transac/revertir")] HttpRequestData httpReq,
        FunctionContext context)
        {
            ExecReverseReq requestApi = await httpReq.ToJsonRequest<ExecReverseReq>();
            DateTime start = ToolHelper.GetActualPeruHour();
            var authResponse = await TaskExtension.ValidarTokenAsync<ExecReverseReq, ExecReverseRes>(dps, 
                httpReq, requestApi, start, "Reversa", requestApi.idEmpresa, HttpStatusCode.Accepted);
            if (authResponse != null) return authResponse;
            var (responseApi, statusCode) = await ats.ExecReverseAsync(requestApi);
            return await TaskExtension.ProcesarResultadoAsync<ExecReverseReq, ExecReverseRes>(
                                dps,
                                httpReq,
                                requestApi,
                                responseApi,
                                "Reversa",
                                start,
                                requestApi.idEmpresa,
                                "Info",
                                HttpStatusCode.Accepted);
        }
    }
}
