using DoAnWebThiTracNghiem.Models;

namespace DoAnWebThiTracNghiem.Areas.Admin.Models
{
    public class ClassDetailViewModel
    {
        public ClassTn Class { get; set; }
        public List<Student_Class> Students { get; set; }
        public List<Exam_Class> Exams { get; set; }
        public List<Notification> Notifications { get; set; }
    }
}
