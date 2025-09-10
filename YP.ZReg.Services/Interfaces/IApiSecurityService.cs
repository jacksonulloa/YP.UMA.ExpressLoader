using System.Net;
using YP.ZReg.Dtos.Contracts.Request;
using YP.ZReg.Dtos.Contracts.Response;

namespace YP.ZReg.Services.Interfaces
{
    public interface IApiSecurityService
    {
        Task<(GetTokenRes, HttpStatusCode)> GetTokenWithClaim(GetTokenReq request);
    }
}