using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DoAnWebThiTracNghiem.Models
{
    public class Exam_Result
    {

        [Key]
        public int Result_ID { get; set; }
        [ForeignKey("Exam")]
        public int Exam_ID { get; set; }
        [ForeignKey("User")]
        public int User_ID { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public decimal Score { get; set; }
        public DateTime Start_Time { get; set; }
        public DateTime End_Time { get; set; }

        public  Exam? Exam { get; set; }
        public  Users? User { get; set; }

        //Điều hướng n-n
        
        public ICollection<Student_Answers>? Student_Answers { get; set; }
    }
}
