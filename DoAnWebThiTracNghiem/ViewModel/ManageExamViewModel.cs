using DoAnWebThiTracNghiem.Models;
using System.ComponentModel.DataAnnotations;

namespace DoAnWebThiTracNghiem.ViewModel
{
    public class ManageExamViewModel
    {
        public Exam Exam { get; set; }
        public List<Exam_Question> ExamQuestions { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn ít nhất một câu hỏi")]
        public List<Question> AvailableQuestions { get; set; }
    }
}
