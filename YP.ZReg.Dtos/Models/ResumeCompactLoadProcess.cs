namespace YP.ZReg.Dtos.Models
{
    public class ResumeCompactLoadProcess
    {
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string TotalRecords { get; set; } = string.Empty;
        public string SuccessRecords { get; set; } = string.Empty;
        public string ErrorRecords { get; set; } = string.Empty;
        public DateTime StartExec { get; set; } = DateTime.MinValue;
        public DateTime EndExec { get; set; } = DateTime.MinValue;
        public string Duration { get; set; } = string.Empty;
    }
}
