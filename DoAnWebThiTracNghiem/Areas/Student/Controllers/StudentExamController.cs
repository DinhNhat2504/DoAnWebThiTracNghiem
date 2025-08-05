using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnWebThiTracNghiem.Areas.Student.Controllers
{
    [Area("Student")]
    public class StudentExamController : Controller
    {
        private readonly AppDBContext _context;
        public StudentExamController(AppDBContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string search, string status, DateTime? from, DateTime? to)
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId"));
            var classIds = await _context.ClassStudents
                .Where(sc => sc.User_ID == userId)
                .Select(sc => sc.Class_ID)
                .ToListAsync();

            var examClasses = await _context.ClassExams
                .Include(ec => ec.Exam)
                .Include(ec => ec.ClassTN)
                .Where(ec => classIds.Contains(ec.ClassTNClass_Id))
                .OrderByDescending(ec => ec.Exam.Exam_Date)
                .ToListAsync();

            var examResults = await _context.ExamResult
                .Where(er => er.User_ID == userId)
                .ToListAsync();

            var exams = examClasses.Select(ec => new LatestExamDisplayViewModel
            {
                Exam = ec.Exam,
                ClassName = ec.ClassTN.ClassName
            }).ToList();

            // Chuyển đổi giờ về giờ Việt Nam
            foreach (var item in exams)
            {
                item.Exam.StartTime = ToVietnamTime(item.Exam.StartTime);
                item.Exam.EndTime = ToVietnamTime(item.Exam.EndTime);
            }

            // Lọc theo tìm kiếm
            if (!string.IsNullOrEmpty(search))
                exams = exams.Where(e => e.Exam.Exam_Name.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            // Lọc theo trạng thái
            var now = DateTime.Now;
            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                exams = exams.Where(e =>
                {
                    var hasTaken = examResults.Any(r => r.Exam_ID == e.Exam.Exam_ID);
                    if (status == "done") return hasTaken;
                    if (status == "expired") return !hasTaken && (now.Date > e.Exam.Exam_Date || (now.Date == e.Exam.Exam_Date && now > e.Exam.EndTime));
                    if (status == "waiting") return !hasTaken && (now.Date < e.Exam.Exam_Date || (now.Date == e.Exam.Exam_Date && now < e.Exam.StartTime));
                    return true;
                }).ToList();
            }

            // Lọc theo khoảng thời gian
            if (from.HasValue)
                exams = exams.Where(e => e.Exam.Exam_Date >= from.Value).ToList();
            if (to.HasValue)
                exams = exams.Where(e => e.Exam.Exam_Date <= to.Value).ToList();

            var vm = new StudentExamFilterViewModel
            {
                Search = search,
                Status = status,
                FromDate = from,
                ToDate = to,
                Exams = exams
            };
            ViewData["ExamResults"] = examResults;
            ViewData["UserId"] = userId;
            return View(vm);
        }
        private DateTime ToVietnamTime(DateTime utcDateTime)
        {
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            // Đảm bảo đầu vào là UTC
            var utc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, vnTimeZone);
        }
    }
}
