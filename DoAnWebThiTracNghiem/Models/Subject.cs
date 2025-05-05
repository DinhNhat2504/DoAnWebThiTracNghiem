using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DoAnWebThiTracNghiem.Models
{
    public class Subject
    {
        [Key]
        public int Subject_Id { get; set; }
        public string? Subject_Name { get; set; }
        [ForeignKey("Users")]
        public int CreatorUser_Id { get; set; }
        public Users? Creator { get; set; }
        [JsonIgnore]
        public ICollection<Exam>? Exams { get; set; }
        [JsonIgnore]
        public ICollection<Question>? Questions { get; set; }
    }
}
