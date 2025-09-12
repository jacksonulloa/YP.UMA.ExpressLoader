namespace YP.ZReg.Dtos.Contracts.Request
{
    public class ExecPaymentReq
    {
        public string fechaTxn { get; set; } = string.Empty;
        public string horaTxn { get; set; } = string.Empty;
        public string idCanal { get; set; } = string.Empty;
        public string idForma { get; set; } = string.Empty;
        public string numeroOperacion { get; set; } = string.Empty;
        public string idConsulta { get; set; } = string.Empty;
        public string servicio { get; set; } = string.Empty;
        public string numeroDocumento { get; set; } = string.Empty;
        public decimal importePagado { get; set; }
        public string moneda { get; set; } = string.Empty;
        public string idEmpresa { get; set; } = string.Empty;
        public string idBanco { get; set; } = string.Empty;
        public string voucher { get; set; } = string.Empty;
        public long referenciaDeuda { get; set; }
    }
}
