using System.Net;
using YP.ZReg.Dtos.Contracts.Request;
using YP.ZReg.Dtos.Contracts.Response;

namespace YP.ZReg.Services.Interfaces
{
    public interface IApiTransacService
    {
        Task<(ExecPaymentRes, HttpStatusCode)> ExecPaymentAsync(ExecPaymentReq request);
        Task<(ExecReverseRes, HttpStatusCode)> ExecReverseAsync(ExecReverseReq request);
        Task<(GetDebtsRes, HttpStatusCode)> GetDebtsAsync(GetDebtsReq request);
    }
}