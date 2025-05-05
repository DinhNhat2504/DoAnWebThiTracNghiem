using DoAnWebThiTracNghiem.Models;

namespace DoAnWebThiTracNghiem.ViewModel
{
    public class ClassDetailsViewModel
    {
        public ClassTn Class { get; set; }
        public List<Notification> Notifications { get; set; }
        public List<Exam_Class> Exams { get; set; }
        public List<Users> Students { get; set; }
        public List<Exam> AvailableExams { get; set; } = new List<Exam>(); // Khởi tạo danh sách trống
    }
}
