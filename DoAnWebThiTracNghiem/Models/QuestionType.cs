using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DoAnWebThiTracNghiem.Models
{
    public class QuestionType
    {
        [Key]
        public int Id { get; set; } // Khóa chính
        public string Name { get; set; } // Tên loại câu hỏi (ví dụ: "1 đáp án", "đúng sai", "điền từ")
        public string? Description { get; set; } // Mô tả (tùy chọn)

        // Liên kết với bảng Question
        [JsonIgnore]
        public ICollection<Question>? Questions { get; set; }
    }
}
