using System.Net;
using YP.ZReg.Entities.Generic;

namespace YP.ZReg.Utils.Interfaces
{
    public interface IBlobLogService
    {
        Task RegistrarLogAsync<TRequest, TResponse>(BlobTableRecord record, TRequest request, TResponse? response, HttpStatusCode statusCode);
    }
}