using YP.ZReg.Entities.Generic;

namespace YP.ZReg.Dtos.Contracts.Response
{
    public class ExecPaymentRes : BaseResponse
    {
        public string cliente { get; set; } = string.Empty;
        public string operacionErp { get; set; } = string.Empty;
    }
}
