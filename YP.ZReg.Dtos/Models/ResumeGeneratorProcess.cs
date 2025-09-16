namespace YP.ZReg.Dtos.Models
{
    public class ResumeGeneratorProcess
    {
        public string idEmpresa { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public List<long> errorRecordIds { get; set; } = [];
        public List<long> okRecordIds { get; set; } = [];
    }
}
