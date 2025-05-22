namespace DoAnWebThiTracNghiem.ViewModel
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public string Search { get; set; }
         public int? SubjectId { get; set; }
        public int? LevelId { get; set; }
        public int? QuestionTypeId { get; set; }
    }
}
