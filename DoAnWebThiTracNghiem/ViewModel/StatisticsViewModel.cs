using DoAnWebThiTracNghiem.Models;

namespace DoAnWebThiTracNghiem.ViewModel
{
    public class StatisticsViewModel
    {
        public int TotalClasses { get; set; }
        public int ActiveClasses { get; set; }
        public int InactiveClasses { get; set; }
        public string? ClassWithMostStudents { get; set; }

        // Bài thi
        public int TotalExams { get; set; }
        public int ActiveExams { get; set; }
        public int InactiveExams { get; set; }

        // Câu hỏi
        public int TotalQuestions { get; set; }
        public List<QuestionTypeStatistics> QuestionsByType { get; set; } = new();

        // Môn học
        public List<SubjectStatistics> Subjects { get; set; } = new();

        // Sinh viên
        public StudentStatistics? TopStudentByExams { get; set; }
        public StudentStatistics? TopStudentByAverageScore { get; set; }
        public List<ClassTn> Classes { get; set; }
        public int? SelectedClassId { get; set; }
        public List<StudentStatistics> Top10StudentsByScore { get; set; } = new();
    }
}
