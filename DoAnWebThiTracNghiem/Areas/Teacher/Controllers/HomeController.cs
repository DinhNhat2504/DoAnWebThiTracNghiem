using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Repositories;
using DoAnWebThiTracNghiem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWebThiTracNghiem.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    public class HomeController : Controller
    {
        private readonly IUserRepository _Ucontext;
        private readonly AppDBContext _Dbcontext;
        private readonly EmailService _emailService;

        // Hàm khởi tạo các biến cần thiết 
        public HomeController(IUserRepository Ucontext, AppDBContext Dbcontext, EmailService emailService)
        {
            _Ucontext = Ucontext;
            _Dbcontext = Dbcontext;
            _emailService = emailService;
        }
        public IActionResult Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole) || userRole != "Teacher")
            {
                return RedirectToAction("Login", "Users", new { area = "" });
            }

            // Lấy UserId của giáo viên từ session
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Users", new { area = "" });
            }

            var model = new ViewModel.TeacherDashboardViewModel
            {
                TotalExams = _Dbcontext.Exams.Count(e => e.CreatorUser_Id == userId),
                TotalClasses = _Dbcontext.ClassTn.Count(c => c.CreatorUser_Id == userId),
                TotalSubjects = _Dbcontext.Subjects.Count(s => s.CreatorUser_Id == userId),
                TotalQuestions = _Dbcontext.Question.Count(q => q.CreatorUser_Id == userId),
                RecentExams = _Dbcontext.Exams
                    .Where(e => e.CreatorUser_Id == userId)
                    .OrderByDescending(e => e.Exam_Date)
                    .Take(3)
                    .ToList(),
                RecentClasses = _Dbcontext.ClassTn
                    .Where(c => c.CreatorUser_Id == userId)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(3)
                    .ToList(),
                RecentQuestion = _Dbcontext.Question.Where(q => q.CreatorUser_Id == userId).OrderByDescending(c => c.CreatedAt).Take(3).ToList(),
                RecentSubject = _Dbcontext.Subjects.Where(s => s.CreatorUser_Id == userId).OrderByDescending(s => s.CreateAt).Take(3).ToList()
            };

            return View(model);
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Users");
        }

    }
}

