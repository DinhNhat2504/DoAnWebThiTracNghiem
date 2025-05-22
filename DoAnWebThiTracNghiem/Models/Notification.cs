using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DoAnWebThiTracNghiem.Models
{
    public class Notification
    {
        [Key]
        public int Announcement_ID { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập nội dung thông báo !")]
        [StringLength(500, ErrorMessage = "Nội dung thông báo không được vượt quá 100 ký tự !")]
        public string? Content { get; set; }
        public DateTime Timestamp { get; set; }
        [ForeignKey("Users")]
        public int CreatorUser_Id { get; set; }
        [ForeignKey("ClassTn")]
        public int ClassTNClass_Id { get; set; }
        public  Users? Creator { get; set; }
        public  ClassTn? ClassTN { get; set; }
    }
}
