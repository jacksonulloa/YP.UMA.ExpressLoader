using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace YP.ZReg.Utils.Extensions
{
    public static class HttpResponseExtensions
    {
        public static async Task<HttpResponseData> ToJsonResponse<T>(this HttpRequestData req, T data, HttpStatusCode status = HttpStatusCode.OK)
        {
            var response = req.CreateResponse(status);
            await response.WriteAsJsonAsync(data);
            return response;
        }
    }
}
