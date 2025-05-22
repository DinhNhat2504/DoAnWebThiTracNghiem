using DoAnWebThiTracNghiem.Repositories;
using DoAnWebThiTracNghiem.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWebThiTracNghiem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IClassTnRepository _classRepo;
        private readonly IExamRepository _examRepo;
        private readonly IQuestionRepository _questionRepo;
        private readonly ISubjectRepository _subjectRepo;

        public HomeController(
            IUserRepository userRepo,
            IClassTnRepository classRepo,
            IExamRepository examRepo,
            IQuestionRepository questionRepo,
            ISubjectRepository subjectRepo)
        {
            _userRepo = userRepo;
            _classRepo = classRepo;
            _examRepo = examRepo;
            _questionRepo = questionRepo;
            _subjectRepo = subjectRepo;
        }

        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole) || userRole != "Admin")
            {
                return RedirectToAction("Login", "Users", new { area = "" });
            }

            var totalStudents = await _userRepo.CountByRoleAsync("Student");
            var totalTeachers = await _userRepo.CountByRoleAsync("Teacher");
            var totalUsers = totalStudents + totalTeachers;
            var totalClasses = await _classRepo.CountAllAsync();
            var totalExams = await _examRepo.CountAllAsync();
            var totalQuestions = await _questionRepo.CountAllAsync();
            var totalSubjects = await _subjectRepo.CountAllAsync();

            // Lấy các user mới tạo tài khoản trong 7 ngày gần nhất (hoặc số lượng mới nhất)
            var newUsers = await _userRepo.GetRecentUsersAsync(10);

            var notifications = newUsers.Select(u => new DashboardNotification
            {
                UserName = u.FullName,
                Role = u.Role.Id == 1 ? "Admin" : (u.Role.Name == "Teacher" ? "Giáo viên" : "Sinh viên"),
                CreatedAt = u.CreatedAt
            }).ToList();

            var model = new HomeDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalStudents = totalStudents,
                TotalTeachers = totalTeachers,
                TotalClasses = totalClasses,
                TotalExams = totalExams,
                TotalQuestions = totalQuestions,
                TotalSubjects = totalSubjects,
                Notifications = notifications
            };

            return View(model);
        }
    
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Users", new { area = "" });
        }
    }
}
