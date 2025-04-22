using System.ComponentModel.DataAnnotations;

namespace DoAnWebThiTracNghiem.Models
{
    public class Level
    {
        [Key]
        public int Id { get; set; }
        public string? LevelName { get; set; }
    }
}
