using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnWebThiTracNghiem.Models
{
    public class Student_Class
    {
        [Key]
        public int SC_ID { get; set; }

        public int User_ID { get; set; } // FK đến bảng Users
        public int Class_ID { get; set; } // FK đến bảng ClassTn

        [ForeignKey("User_ID")]
        public Users? User { get; set; }

        [ForeignKey("Class_ID")]
        public ClassTn? ClassTn { get; set; }
    }
}
