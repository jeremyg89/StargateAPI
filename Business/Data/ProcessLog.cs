using System.ComponentModel.DataAnnotations.Schema;

namespace StargateAPI.Business.Data
{
    [Table("ProcessLog")]
    public class ProcessLog
    {
        public int Id { get; set; }
        public DateTime LoggedAt { get; set; }
        public bool Success { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
