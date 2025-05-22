using DoAnWebThiTracNghiem.Models;

namespace DoAnWebThiTracNghiem.Areas.Admin.Models
{
    public class UserDetailViewModel
    {
        public Users User { get; set; }
        public int CreatedClassCount { get; set; }
        public int CreatedExamCount { get; set; }
        public int CreatedSubjectCount { get; set; }
        public int CreatedQuestionCount { get; set; }
        public int JoinedClassCount { get; set; }
        public int TakenExamCount { get; set; }
    }
   
}
