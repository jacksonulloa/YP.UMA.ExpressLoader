using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.OpenApi.Models;

namespace YP.ZReg.Utils.Extensions
{
    public class OpenApiConfigurationOptions : DefaultOpenApiConfigurationOptions
    {
        public override OpenApiInfo Info => new()
        {
            Title = "API TRANSACCIONAL",
            Version = "v1",
            Description = "API de integracion a la plataforma YAPAGO"
        };
    }
}
