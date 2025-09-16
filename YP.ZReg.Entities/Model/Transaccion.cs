namespace YP.ZReg.Entities.Model
{
    public class Transaccion
    {
        public long id { get; set; } = 0;
        public DateTime fecha_hora_transaccion { get; set; } = DateTime.Now;
        public string id_canal_pago { get; set; } = string.Empty;
        public string id_forma_pago { get; set; } = string.Empty;
        public string numero_operacion { get; set; } = string.Empty;
        public string id_consulta { get; set; } = string.Empty;
        public string servicio { get; set; } = string.Empty;
        public string numero_documento { get; set; } = string.Empty;
        public decimal importe_pagado { get; set; } = 0.00m;
        public string moneda { get; set; } = string.Empty;
        public string id_empresa { get; set; } = string.Empty;
        public long id_deuda { get; set; }
        /// <summary>
        /// P: Pay | R: Reverse
        /// </summary>
        public string tipo_transac { get; set; } = string.Empty;
        public string id_banco { get; set; } = string.Empty;
        public string voucher{ get; set; } = string.Empty;
        public string cuenta_banco { get; set; } = string.Empty;
        public string nombre_cliente { get; set; } = string.Empty;

        /// <summary>
        /// P:Pending | S:Sent
        /// </summary>
        public string estado_notificacion { get; set; } = string.Empty;
    }
}
