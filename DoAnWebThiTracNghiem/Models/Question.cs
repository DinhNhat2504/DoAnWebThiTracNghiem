using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DoAnWebThiTracNghiem.Models
{
    public class Question
    {
        [Key]
        public int Question_ID { get; set; }
        public string? Question_Content { get; set; }
        public string? Option_A { get; set; }
        public string? Option_B { get; set; }
        public string? Option_C { get; set; }
        public string? Option_D { get; set; }
        public string? Correct_Option { get; set; }
        [ForeignKey("Subject")]
        public int Subject_ID { get; set; }
        public Subject? Subject { get; set; }
        [ForeignKey("Level")]
        public int Level_ID { get; set; }
        public Level? Level { get; set; }
        [ForeignKey("Users")]
        public int CreatorUser_Id { get; set; }
        public Users? Creator { get; set; }
        public DateTime CreatedAt { get; set; }

        //Điều hướng n-n
        public ICollection<Student_Answers>? Student_Answers { get; set; }
        public ICollection<Exam_Question>? Exam_Questions { get; set; }
    }
}
