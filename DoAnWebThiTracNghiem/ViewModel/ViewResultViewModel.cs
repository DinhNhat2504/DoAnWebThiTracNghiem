using DoAnWebThiTracNghiem.Models;

namespace DoAnWebThiTracNghiem.ViewModel
{
    public class ViewResultViewModel
    {
      
            public Exam Exam { get; set; }
            public decimal Score { get; set; }
            public int CorrectAnswers { get; set; }
            public int WrongAnswers { get; set; }
            public List<Student_Answers> StudentAnswers { get; set; }
            public decimal Pass { get; set; }
        

    }
}
