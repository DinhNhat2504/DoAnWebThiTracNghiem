using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnWebThiTracNghiem.Models
{
    public class Exam_Class
    {
        [Key]
        public int EC_ID { get; set; }
        [ForeignKey("Exam")]
        [Required(ErrorMessage = "Vui lòng chọn 1 bài thi !")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn bài thi hợp lệ")]
        public int Exam_ID { get; set; }
        [ForeignKey("ClassTn")]
        public int ClassTNClass_Id { get; set; }
        public Exam? Exam { get; set; }
        public ClassTn? ClassTN { get; set; }
        public DateTime AssignedAt { get; set; }
        
    }
}
