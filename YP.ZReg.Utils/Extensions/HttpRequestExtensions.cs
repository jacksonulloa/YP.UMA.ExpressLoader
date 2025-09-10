using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

namespace YP.ZReg.Utils.Extensions
{
    public static class HttpRequestExtensions
    {
        public static async Task<T> ToJsonRequest<T>(this HttpRequestData req) where T : new()
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            T requestApi = JsonConvert.DeserializeObject<T>(body) ?? new();
            return requestApi;
        }
    }
}
