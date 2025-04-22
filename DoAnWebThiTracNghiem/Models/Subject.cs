using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnWebThiTracNghiem.Models
{
    public class Subject
    {
        [Key]
        public int Subject_Id { get; set; }
        public string? Subject_Name { get; set; }
        [ForeignKey("Users")]
        public int CreatorUser_Id { get; set; }
        public Users? Creator { get; set; }

    }
}
