using System.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace DoAnWebThiTracNghiem.Models
{
    public class Question
    {
        [Key]
        public int Question_ID { get; set; }

        [Required(ErrorMessage = "Nội dung câu hỏi là bắt buộc")]
        [StringLength(1000, ErrorMessage = "Nội dung câu hỏi không được vượt quá 1000 ký tự")]
        public string? Question_Content { get; set; }

        [ForeignKey("QuestionType")]
        [Required(ErrorMessage = "Loại câu hỏi là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn loại câu hỏi hợp lệ")]
        public int QuestionTypeId { get; set; }

        public QuestionType? QuestionType { get; set; }
        [OptionsValidation]
        [JsonIgnore]
        public List<string>? Options { get; set; }

        [Required(ErrorMessage = "Đáp án đúng là bắt buộc")]
        [StringLength(500, ErrorMessage = "Đáp án đúng không được vượt quá 500 ký tự")]
        public string? Correct_Option { get; set; }

        [ForeignKey("Subject")]
        [Required(ErrorMessage = "Môn học là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn môn học hợp lệ")]
        public int Subject_ID { get; set; }

        public Subject? Subject { get; set; }

        [ForeignKey("Level")]
        [Required(ErrorMessage = "Mức độ là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn mức độ hợp lệ")]
        public int Level_ID { get; set; }

        public Level? Level { get; set; }

        [ForeignKey("Users")]
        public int CreatorUser_Id { get; set; }

        public Users? Creator { get; set; }

        public DateTime CreatedAt { get; set; }

        // Điều hướng n-n
        public ICollection<Student_Answers>? Student_Answers { get; set; }

        [JsonIgnore]
        public ICollection<Exam_Question>? Exam_Questions { get; set; }
    }
}
