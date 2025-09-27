namespace YP.ZReg.Entities.Model
{
    public class Servicio
    {
        public int id { get; set; }
        public int id_empresa { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public int moneda { get; set; }
        public string tipo_validacion { get; set; } = string.Empty;
        public string tipo_pago { get; set; } = string.Empty;
        public int estado { get; set; }
        public string numero_cuenta { get; set; } = string.Empty;
    }
}
