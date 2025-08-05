using DoAnWebThiTracNghiem.Models;

namespace DoAnWebThiTracNghiem.ViewModel
{
    public class AdminStatisticsViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<StatPoint> UserStats { get; set; }
        public List<StatPoint> TeacherStats { get; set; }
        public List<StatPoint> StudentStats { get; set; }
        public List<StatPoint> ClassStats { get; set; }
        public List<StatPoint> ExamStats { get; set; }
        public List<StatPoint> QuestionStats { get; set; }
        public List<StatPoint> SubjectStats { get; set; }
        public List<ClassTn> ActiveClasses { get; set; }
        public List<ClassTn> InactiveClasses { get; set; }

        // Thêm các thuộc tính tổng số
        public int TotalTeachers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalClasses { get; set; }
        public int TotalExams { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalSubjects { get; set; }
    }
    public class StatPoint
    {
        public string Date { get; set; } // "yyyy-MM-dd"
        public int Count { get; set; }
    }

}
