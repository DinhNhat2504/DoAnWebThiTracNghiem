namespace DoAnWebThiTracNghiem.Areas.Teacher.Models
{
    public class ExamResultListItemViewModel
    {
        public int ResultId { get; set; }
        public string StudentName { get; set; }
        public decimal Score { get; set; }
        public bool IsPassed { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
    }


    public class ExamResultsViewModel
    {
        public string ExamName { get; set; }
        public decimal PassScore { get; set; }
        public List<ExamResultListItemViewModel> Results { get; set; }
        public string Search { get; set; }
        public string PassFilter { get; set; }
        public string SortOrder { get; set; }
    }

}
