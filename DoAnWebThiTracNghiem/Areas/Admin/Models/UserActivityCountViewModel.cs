using DoAnWebThiTracNghiem.Models;

namespace DoAnWebThiTracNghiem.Areas.Admin.Models
{
    public class UserActivityViewModel
    {
        public Users User { get; set; }
        public int ExamCount { get; set; }
        public int SubjectCount { get; set; }
        public int QuestionCount { get; set; }
        public int ClassCount { get; set; }
        // Thêm cho học sinh
        public int JoinedClassCount { get; set; }
        public int TakenExamCount { get; set; }
    }
    public class UserActivityCountViewModel
    {
        public int UserId { get; set; }
        public int ExamCount { get; set; }
        public int SubjectCount { get; set; }
        public int QuestionCount { get; set; }
        public int ClassCount { get; set; }
        // Thêm cho học sinh
        public int JoinedClassCount { get; set; }
        public int TakenExamCount { get; set; }
    }
}
