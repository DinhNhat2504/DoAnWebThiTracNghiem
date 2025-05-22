using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DoAnWebThiTracNghiem.Models
{
    public class Subject
    {
        [Key]
        public int Subject_Id { get; set; }

        [Required(ErrorMessage = "Tên môn học là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên môn học không được vượt quá 100 ký tự")]
        public string? Subject_Name { get; set; }
        public DateTime CreateAt { get; set; }
        [ForeignKey("Users")]
        public int CreatorUser_Id { get; set; }
        public Users? Creator { get; set; }
        [JsonIgnore]
        public ICollection<Exam>? Exams { get; set; }
        [JsonIgnore]
        public ICollection<Question>? Questions { get; set; }
    }
}
