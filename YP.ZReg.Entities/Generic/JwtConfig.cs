namespace YP.ZReg.Entities.Generic
{
    public class JwtConfig
    {
        public string Claim { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty; 
    }
}
