using YP.ZReg.Entities.Generic;

namespace YP.ZReg.Dtos.Contracts.Response
{
    public class GetDebtsRes : BaseResponse
    {
        public string cliente {  get; set; } = string.Empty;
        public List<Debt> deudas { get; set; } = [];
    }
    public class Debt
    {
        public string idDeuda { get; set; } = string.Empty;
        public string servicio { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public string descripcionDoc { get; set; } = string.Empty;
        public string fechaVencimiento { get; set; } = string.Empty;
        public string fechaEmision { get; set; } = string.Empty;
        public string deuda { get; set; } = string.Empty;
        public string pagoMinimo { get; set; } = string.Empty;
        public string moneda { get; set; } = string.Empty;
    }
}
