using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnWebThiTracNghiem.Models
{
    public class Exam_Question
    {
        [Key]
        public int EQ_ID { get; set; }
        [ForeignKey("Exam")]
        public int Exam_ID { get; set; }
        
        public Exam? Exam { get; set; }
        [ForeignKey("Question")]
        [Required(ErrorMessage = "Vui lòng chọn 1 câu hỏi!")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn câu hỏi hợp lệ")]
        public int Question_ID { get; set; }
       
        public Question? Question { get; set; }
        public int? Question_Order { get; set; }
        public decimal Points { get; set; }
    }
}
