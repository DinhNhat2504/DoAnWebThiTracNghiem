using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DoAnWebThiTracNghiem.Models
{
    public class Exam
    {
        [Key]
        public int Exam_ID { get; set; }
        [Required(ErrorMessage = "The Exam Name field is required.")]
        public string? Exam_Name { get; set; }
        public int TotalQuestions { get; set; }
        public int Duration { get; set; }
        public double PassScore { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime Exam_Date { get; set; }
        public bool IsActive { get; set; }
     
        [ForeignKey("Users")]
        public int CreatorUser_Id { get; set; }
        public virtual Users? Creator { get; set; }
        [ForeignKey("Subject")]
        public int Subject_ID { get; set; }
        public virtual Subject? Subject { get; set; }

        //Điều hướng n-n
        public ICollection<Exam_Class>? Exam_Classes { get; set; }
        public ICollection<Exam_Question>? Exam_Questions { get; set; }
        public ICollection<Exam_Result>? exam_Results { get; set; }
    }
}
