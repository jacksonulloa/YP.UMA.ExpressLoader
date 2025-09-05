namespace YP.ZReg.Entities.Model
{
    public class Deuda
    {
        public long id { get; set; }
        public string numero_documento { get; set; } = string.Empty;
        public string llave_principal { get; set; } = string.Empty;
        public string dni { get; set; } = string.Empty;
        public string ruc { get; set; } = string.Empty;
        public string llave_alterna { get; set; } = string.Empty;
        public DateTime fecha_vencimiento { get; set; }
        public DateTime fecha_emision { get; set; }
        public decimal importe_bruto { get; set; }
        public decimal mora { get; set; }
        public decimal gasto_administrativo { get; set; }
        public decimal importe_minimo { get; set; }
        //public string glosa { get; set; } = string.Empty;
        public string nombre_cliente { get; set; } = string.Empty;
        public string servicio { get; set; } = string.Empty;
        public string moneda { get; set; } = string.Empty;
        public string periodo { get; set; } = string.Empty;
        public string cuota { get; set; } = string.Empty;
        public string anio { get; set; } = string.Empty;
        public string id_empresa { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
    }
}
