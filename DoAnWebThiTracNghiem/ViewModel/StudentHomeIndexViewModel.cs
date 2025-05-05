using DoAnWebThiTracNghiem.Models;

namespace DoAnWebThiTracNghiem.ViewModel
{
    public class StudentHomeIndexViewModel
    {
        public List<ClassTn> RecentClasses { get; set; } = new();
        public List<Exam> RecentExams { get; set; } = new();
        public Exam_Result? HighestScoreExam { get; set; }
        public ClassTn? MostActiveClass { get; set; }
        public int MostActiveClassExamCount { get; set; }
    }
}
