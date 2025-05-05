using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.ViewModel;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userRole) || userRole != "Student" || string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login", "Users", new { area = "" });
            }
            int userId = int.Parse(userIdStr);

            // 1. 3 most recent classes joined
            var recentClasses = _context.ClassStudents
                .Where(sc => sc.User_ID == userId)
                .OrderByDescending(sc => sc.Timestamp)
                .Select(sc => sc.ClassTn)
                .Take(3)
                .ToList();

            // 2. 3 most recent exams assigned to those classes
            var recentClassIds = recentClasses.Select(c => c.Class_Id).ToList();
            var recentExams = _context.ClassExams
                .Where(ec => recentClassIds.Contains(ec.ClassTNClass_Id))
                .OrderByDescending(ec => ec.AssignedAt)
                .Select(ec => ec.Exam)
                .Distinct()
                .Take(3)
                .ToList();

            // 3. Highest score exam for this student
            var highestScoreExam = _context.ExamResult
                .Where(er => er.User_ID == userId)
                .OrderByDescending(er => er.Score)
                .FirstOrDefault();

            // 4. Most active class (where student did most exams)
            var classExamCounts = _context.ExamResult
                .Where(er => er.User_ID == userId && er.Exam != null)
                .Select(er => new
                {
                    er.Exam,
                    er.Exam.Exam_Classes
                })
                .SelectMany(x => x.Exam.Exam_Classes, (x, ec) => new { ec.ClassTNClass_Id })
                .GroupBy(x => x.ClassTNClass_Id)
                .Select(g => new { ClassId = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .FirstOrDefault();

            ClassTn? mostActiveClass = null;
            int mostActiveClassExamCount = 0;
            if (classExamCounts != null)
            {
                mostActiveClass = _context.ClassTn.FirstOrDefault(c => c.Class_Id == classExamCounts.ClassId);
                mostActiveClassExamCount = classExamCounts.Count;
            }

            var vm = new StudentHomeIndexViewModel
            {
                RecentClasses = recentClasses,
                RecentExams = recentExams,
                HighestScoreExam = highestScoreExam,
                MostActiveClass = mostActiveClass,
                MostActiveClassExamCount = mostActiveClassExamCount
            };

            return View(vm);
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Users", new { area = "" });
        }
    }
}
