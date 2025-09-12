namespace YP.ZReg.Dtos.Contracts.Request
{
    public class ExecReverseReq
    {
        public string fechaTxn { get; set; } = string.Empty;
        public string horaTxn { get; set; } = string.Empty;
        public string idBanco { get; set; } = string.Empty;
        public string idConsulta { get; set; } = string.Empty;
        public string idServicio { get; set; } = string.Empty;
        public string tipoConsulta { get; set; } = string.Empty;
        public string numeroOperacion { get; set; } = string.Empty;
        public string numeroDocumento { get; set; } = string.Empty;
        public string idEmpresa { get; set; } = string.Empty;
        public string voucher { get; set; } = string.Empty;
    }
}
