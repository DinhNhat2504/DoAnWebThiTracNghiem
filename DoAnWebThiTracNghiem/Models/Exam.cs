using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DoAnWebThiTracNghiem.Models
{
    public class Exam
    {
        [Key]
        public int Exam_ID { get; set; }

        [Required(ErrorMessage = "Tên kỳ thi là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên kỳ thi không được vượt quá 200 ký tự")]
        public string? Exam_Name { get; set; }

        [Required(ErrorMessage = "Số lượng câu hỏi là bắt buộc")]
        [Range(1,200, ErrorMessage = "Số lượng câu hỏi phải lớn hơn 0, tối đa là 200")]
        public int TotalQuestions { get; set; }
         
        [Required(ErrorMessage = "Thời gian làm bài là bắt buộc")]
        [Range(1, 180, ErrorMessage = "Thời gian làm bài phải lớn hơn 0 phút, tối đa là 180 phút")]
        public int Duration { get; set; }

        [Required(ErrorMessage = "Điểm đậu là bắt buộc")]
        [Range(0.0, 100.0, ErrorMessage = "Điểm đậu phải từ 0 đến 100")]
        public double PassScore { get; set; }

        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc")]
        
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc")]
        [StartEndTimeValidation]
        [DataType(DataType.DateTime)]
        public DateTime EndTime { get; set; }

        [Required(ErrorMessage = "Ngày thi là bắt buộc")]
        [DataType(DataType.Date)] 
        public DateTime Exam_Date { get; set; }

        public DateTime CreateAt { get; set; }

        public bool IsActive { get; set; }

        [ForeignKey("Users")]
        public int CreatorUser_Id { get; set; }

        public virtual Users? Creator { get; set; }

        [ForeignKey("Subject")]
        [Required(ErrorMessage = "Môn học là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn môn học hợp lệ")]
        public int Subject_ID { get; set; }

        public virtual Subject? Subject { get; set; }

        // Điều hướng n-n
        public ICollection<Exam_Class>? Exam_Classes { get; set; }
        public ICollection<Exam_Question>? Exam_Questions { get; set; }
        public ICollection<Exam_Result>? exam_Results { get; set; }
    }
}
