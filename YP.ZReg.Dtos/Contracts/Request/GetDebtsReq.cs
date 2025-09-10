namespace YP.ZReg.Dtos.Contracts.Request
{
    public class GetDebtsReq
    {
        public string empresa { get; set; } = string.Empty;
        public string servicio { get; set; } = string.Empty;
        public string id { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string canal { get; set; } = string.Empty;
        public string banco { get; set; } = string.Empty;
    }
}
