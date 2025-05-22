using DoAnWebThiTracNghiem.Models;

namespace DoAnWebThiTracNghiem.ViewModel
{
    public class ClassInfoViewModel
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string Creator { get; set; }
        public int CompletedExams { get; set; }
        public int PendingExams { get; set; }
        public DateTime JoinDate { get; set; }
    }
    public class LatestExamDisplayViewModel
    {
        public Exam Exam { get; set; }
        public string ClassName { get; set; }
    }
}
