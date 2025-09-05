using YP.ZReg.Entities.Generic;

namespace YP.ZReg.Dtos.Models
{
    public class ResumeErrorRecord
    {
        public string Row { get; set; } = string.Empty;
        public List<string> Results { get; set; } = [];
    }
}
