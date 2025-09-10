using YP.ZReg.Entities.Generic;

namespace YP.ZReg.Dtos.Contracts.Response
{
    public class GetTokenRes : BaseResponse
    {
        public string Token { get; set; } = string.Empty;
        public string ExpirationDate { get; set; } = string.Empty;
    }
}
