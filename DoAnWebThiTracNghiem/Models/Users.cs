using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace DoAnWebThiTracNghiem.Models
{
    public class Users
    {
        [Key]
        public int User_Id { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        [ForeignKey("Roles")]
        public int RoleId { get; set; }
        public Roles? Role { get; set; }
        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordTokenExpiry { get; set; }

        //Điều hướng n-n
        public ICollection<Student_Class>? Student_Class { get; set; }

    }
}
