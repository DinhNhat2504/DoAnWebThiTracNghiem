using DoAnWebThiTracNghiem.Models;

namespace DoAnWebThiTracNghiem.ViewModel
{
    public class ManageExamViewModel
    {
        public Exam Exam { get; set; }
        public List<Exam_Question> ExamQuestions { get; set; }
        public List<Question> AvailableQuestions { get; set; }
    }
}
