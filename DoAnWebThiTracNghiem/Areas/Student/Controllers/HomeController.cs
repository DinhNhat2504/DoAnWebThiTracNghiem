using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnWebThiTracNghiem.Areas.Student.Controllers
{
    [Area("Student")]
    public class HomeController : Controller
    {
        private readonly AppDBContext _context;
        public HomeController(AppDBContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userRole) || userRole != "Student" || string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login", "Users", new { area = "" });
            }
            int userId = int.Parse(userIdStr);

            var studentClasses = await _context.ClassStudents
        .Include(sc => sc.ClassTn)
            .ThenInclude(c => c.Creator)
        .Where(sc => sc.User_ID == userId)
        .ToListAsync();
            var exams1 = await _context.ClassExams
                                .Include(ec => ec.ClassTN)
                               

                                .Include(ec => ec.Exam)
                                .ToListAsync();
            // For each class, calculate exam stats
            var classInfos = new List<ClassInfoViewModel>();
            foreach (var sc in studentClasses)
            {
                var exams = await _context.ClassExams
                                .Include(ec => ec.ClassTN)
                                .Where(ec => ec.ClassTNClass_Id == sc.Class_ID)

                                .Select(ec => ec.Exam)
                                .ToListAsync();

                var completed = await _context.ExamResult
                    .CountAsync(er => er.User_ID == userId && exams.Select(e => e.Exam_ID).Contains(er.Exam_ID));
                var pending = exams.Count - completed;

                classInfos.Add(new ClassInfoViewModel
                {
                    ClassId = sc.Class_ID,
                    ClassName = sc.ClassTn.ClassName,
                    Creator = sc.ClassTn.Creator?.FullName,
                    CompletedExams = completed,
                    PendingExams = pending,
                    JoinDate = sc.Timestamp
                });
            }
            // Lấy danh sách ID lớp mà sinh viên đã tham gia
            var classIds = await _context.ClassStudents
                .Where(sc => sc.User_ID == userId)
                .Select(sc => sc.Class_ID)
                .ToListAsync();

            // Lấy các bài thi được giao cho các lớp này
            var examClasses = await _context.ClassExams
                .Include(ec => ec.Exam)
                .Include(ec => ec.ClassTN)
                .Where(ec => classIds.Contains(ec.ClassTNClass_Id))
                .OrderByDescending(ec => ec.Exam.Exam_Date)
                .Take(3)
                .ToListAsync();
            var latestExams = examClasses.Select(ec => new LatestExamDisplayViewModel
            {
                Exam = ec.Exam,
                ClassName = ec.ClassTN.ClassName
            }).ToList();

            // Chuyển đổi giờ về giờ Việt Nam
            foreach (var item in latestExams)
            {
                item.Exam.StartTime = ToVietnamTime(item.Exam.StartTime);
                item.Exam.EndTime = ToVietnamTime(item.Exam.EndTime);
            }

            var examResults = await _context.ExamResult
                .Where(er => er.User_ID == userId)
                .ToListAsync();
            ViewData["ExamResults"] = examResults;
            ViewData["UserId"] = userId;
            ViewData["LatestExams"] = latestExams;
            ViewData["Classes"] = classInfos;
            ViewData["UserID"] = userId;
            
            ViewData["Exams"] = exams1;
            return View();
        }

        private DateTime ToVietnamTime(DateTime utcDateTime)
        {
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            // Đảm bảo đầu vào là UTC
            var utc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, vnTimeZone);
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Users", new { area = "" });
        }
    }
}
