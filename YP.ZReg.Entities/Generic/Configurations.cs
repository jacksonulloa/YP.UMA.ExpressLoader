namespace YP.ZReg.Entities.Generic
{
    public class Configurations
    {
        public List<EmpresaConfig> EmpresasConfig { get; set; } = [];
    }
    public class EmpresaConfig
    {
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public List<string> Servicios { get; set; } = [];
    }
}
