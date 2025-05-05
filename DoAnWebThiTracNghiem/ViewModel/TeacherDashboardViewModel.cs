using DoAnWebThiTracNghiem.Models;

namespace DoAnWebThiTracNghiem.ViewModel
{
    public class TeacherDashboardViewModel
    {
        public int TotalExams { get; set; }
        public int TotalClasses { get; set; }
        public int TotalSubjects { get; set; }
        public int TotalQuestions { get; set; }
        public List<Exam> RecentExams { get; set; } = new();
        public List<ClassTn> RecentClasses { get; set; } = new();
    }
}
