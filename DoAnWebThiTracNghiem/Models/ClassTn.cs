using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnWebThiTracNghiem.Models
{
    public class ClassTn
    {
        [Key]
        public int Class_Id { get; set; }
        public string? ClassName { get; set; }
        public string? InviteCode { get; set; }
        [ForeignKey("Subject")]
        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }
        [ForeignKey("Users")]
        public int CreatorUser_Id { get; set; }
        public Users? Creator { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        //Điều hướng n-n
        public ICollection<Exam_Class>? Exam_Classes { get; set; }
        public ICollection<Student_Class>? Student_Classes { get; set; }
    }
}
