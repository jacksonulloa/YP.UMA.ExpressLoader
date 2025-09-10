namespace YP.ZReg.Entities.Generic
{
    public class Configurations
    {
        public List<EmpresaConfig> EmpresasConfig { get; set; } = [];
        public JwtAlterConfig JwtAlterConfig { get; set; } = new();
    }
    public class EmpresaConfig
    {
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public List<string> Servicios { get; set; } = [];
    }
    public class JwtAlterConfig
    {
        public string Audience { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string MinutesFactor { get; set; } = string.Empty;
        public string HoursFactor { get; set; } = string.Empty;
        public string DaysFactor { get; set; } = string.Empty;
    }
}
