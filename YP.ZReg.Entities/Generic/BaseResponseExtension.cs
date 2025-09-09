namespace YP.ZReg.Entities.Generic
{
    public class BaseResponseExtension : BaseResponse
    {
        public string Resume { get; set; } = string.Empty;
        public DateTime StartExec { get; set; } = DateTime.MinValue;
        public DateTime EndExec { get; set; } = DateTime.MinValue;
        public string Duration { get; set; } = string.Empty;
    }
}
