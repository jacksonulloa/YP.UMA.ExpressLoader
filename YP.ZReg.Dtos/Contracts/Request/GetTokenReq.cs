namespace YP.ZReg.Dtos.Contracts.Request
{
    public class GetTokenReq
    {
        public string UserType { get; set; } = string.Empty;
        public string UserProfile { get; set; } = string.Empty;
        public string UserPass { get; set; } = string.Empty;
    }
}
