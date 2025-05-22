namespace DoAnWebThiTracNghiem.ViewModel
{
    public class HomeDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalClasses { get; set; }
        public int TotalExams { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalSubjects { get; set; }
        public List<DashboardNotification> Notifications { get; set; }
    }

    public class DashboardNotification
    {
        public string UserName { get; set; }
        public string Role { get; set; } // "Giáo viên" hoặc "Sinh viên"
        public DateTime CreatedAt { get; set; }
    }
}
