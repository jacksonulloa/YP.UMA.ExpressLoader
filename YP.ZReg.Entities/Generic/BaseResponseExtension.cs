namespace YP.ZReg.Entities.Generic
{
    public class BaseResponseExtension : BaseResponse
    {
        public string Resume { get; set; } = string.Empty;
        public DateTime StartExec { get; set; }
        public DateTime EndExec { get; set; }
        public string Duration { get; set; } = string.Empty;
    }
}
