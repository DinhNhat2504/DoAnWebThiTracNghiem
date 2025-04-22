using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DoAnWebThiTracNghiem.Models
{
    public class Notification
    {
        [Key]
        public int Announcement_ID { get; set; }
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
