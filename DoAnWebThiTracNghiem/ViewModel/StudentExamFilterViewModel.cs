namespace DoAnWebThiTracNghiem.ViewModel
{
    public class StudentExamFilterViewModel
    {
        public string Search { get; set; }
        public string Status { get; set; } // "all", "done", "waiting", "expired"
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<LatestExamDisplayViewModel> Exams { get; set; }
    }
}
