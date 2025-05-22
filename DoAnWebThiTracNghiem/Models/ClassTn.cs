using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnWebThiTracNghiem.Models
{
    public class ClassTn
    {
        [Key]
        public int Class_Id { get; set; }

        [Required(ErrorMessage = "Tên lớp học là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên lớp học không được vượt quá 100 ký tự")]
        public string? ClassName { get; set; }
        
        public string? InviteCode { get; set; }

        [ForeignKey("Subject")]
        [Required(ErrorMessage = "Môn học là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn môn học hợp lệ")]
        public int SubjectId { get; set; }

        public Subject? Subject { get; set; }

        [ForeignKey("Users")]
        
        public int CreatorUser_Id { get; set; }

        public Users? Creator { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Điều hướng n-n
        public ICollection<Exam_Class>? Exam_Classes { get; set; }
        public ICollection<Student_Class>? Student_Classes { get; set; }
    }
}
