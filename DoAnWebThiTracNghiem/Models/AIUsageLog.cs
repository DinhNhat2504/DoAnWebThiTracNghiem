using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnWebThiTracNghiem.Models
{
    public class AIUsageLog
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Users")]
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public int UsageCount { get; set; }
        public Users? User { get; set; }
    }
}
