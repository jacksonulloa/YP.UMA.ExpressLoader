namespace YP.ZReg.Dtos.Models
{
    public class ResumeLoadProcess: ResumeCompactLoadProcess
    {
        public List<ResumeLoadErrorRecord> ErrorDetails { get; set; } = [];
    }
}
