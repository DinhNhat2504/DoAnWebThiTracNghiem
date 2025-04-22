using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnWebThiTracNghiem.Models
{
    public class Student_Answers
    {
        [Key]
        public int SA_ID { get; set; }
        [ForeignKey("Exam_Result")]
        public int Result_ID { get; set; }
        [ForeignKey("Question")]
        public int Question_ID1 { get; set; }
        public  Exam_Result? Result { get; set; }
        public Question? Question { get; set; }
        public string? Selected_Option { get; set; }
        public bool Is_Correct { get; set; }
    }
}
