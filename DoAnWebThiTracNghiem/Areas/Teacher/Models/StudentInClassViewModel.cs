namespace DoAnWebThiTracNghiem.Areas.Teacher.Models
{
    public class StudentInClassViewModel
    {
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public int ExamsCompleted { get; set; }
        public int ExamsPending { get; set; }
        public int ExamsPassed { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
