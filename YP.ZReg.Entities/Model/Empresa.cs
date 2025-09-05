namespace YP.ZReg.Entities.Model
{
    public class Empresa
    {
        public int id { get; set; }
        public string id_proveedor { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string ruc { get; set; } = string.Empty;
        public int estado { get; set; }
        public List<Servicio> servicios { get; set; } = [];
    }
}
