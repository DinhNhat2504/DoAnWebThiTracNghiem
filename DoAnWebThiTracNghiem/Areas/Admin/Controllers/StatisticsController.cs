using DoAnWebThiTracNghiem.Repositories;
using DoAnWebThiTracNghiem.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWebThiTracNghiem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StatisticsController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IClassTnRepository _classRepo;
        private readonly IExamRepository _examRepo;
        private readonly IQuestionRepository _questionRepo;
        private readonly ISubjectRepository _subjectRepo;

        public StatisticsController(
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
        public async Task<IActionResult> Index(DateTime? start, DateTime? end)
        {
            var endDate = end ?? DateTime.Today;
            var startDate = start ?? endDate.AddDays(-6);

            var users = await _userRepo.GetUsersCreatedBetween(startDate, endDate);
            var classes = await _classRepo.GetClassesCreatedBetween(startDate, endDate);
            var exams = await _examRepo.GetExamsCreatedBetween(startDate, endDate);
            var questions = await _questionRepo.GetQuestionsCreatedBetween(startDate, endDate);
            var subjects = await _subjectRepo.GetSubjectsCreatedBetween(startDate, endDate);

            var activeClasses = await _classRepo.GetActiveClassesAsync();
            var inactiveClasses = await _classRepo.GetInactiveClassesAsync();

            // Tổng số lượng
            var teachers = users.Where(u => u.Role != null && u.Role.Name == "Teacher").ToList();
            var students = users.Where(u => u.Role != null && u.Role.Name == "Student").ToList();

            var totalTeachers = teachers.Count;
            var totalStudents = students.Count;
            var totalClasses = classes.Count;
            var totalExams = exams.Count;
            var totalQuestions = questions.Count;
            var totalSubjects = subjects.Count;


            // Group by date for chart
            var userStats = users.GroupBy(u => u.CreatedAt.Date)
                .Select(g => new StatPoint { Date = g.Key.ToString("yyyy-MM-dd"), Count = g.Count() }).ToList();
            // Lấy danh sách ngày trong khoảng lọc
            var dateRange = Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(offset => startDate.AddDays(offset).ToString("yyyy-MM-dd"))
                .ToList();

            // Thống kê từng ngày
            var teacherStats = dateRange.Select(date => new StatPoint
            {
                Date = date,
                Count = teachers.Count(u => u.CreatedAt.ToString("yyyy-MM-dd") == date)
            }).ToList();

            var studentStats = dateRange.Select(date => new StatPoint
            {
                Date = date,
                Count = students.Count(u => u.CreatedAt.ToString("yyyy-MM-dd") == date)
            }).ToList();
            var classStats = classes.GroupBy(c => c.CreatedAt.Date)
                .Select(g => new StatPoint { Date = g.Key.ToString("yyyy-MM-dd"), Count = g.Count() }).ToList();
            var examStats = exams.GroupBy(e => e.Exam_Date.Date)
                .Select(g => new StatPoint { Date = g.Key.ToString("yyyy-MM-dd"), Count = g.Count() }).ToList();
            var questionStats = questions.GroupBy(q => q.CreatedAt.Date)
                .Select(g => new StatPoint { Date = g.Key.ToString("yyyy-MM-dd"), Count = g.Count() }).ToList();
            var subjectStats = subjects.GroupBy(s => s.CreateAt.Date)
                .Select(g => new StatPoint { Date = g.Key.ToString("yyyy-MM-dd"), Count = g.Count() }).ToList();

            var vm = new AdminStatisticsViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                UserStats = userStats,
                TeacherStats = teacherStats,
                StudentStats = studentStats,
                ClassStats = classStats,
                ExamStats = examStats,
                QuestionStats = questionStats,
                SubjectStats = subjectStats,
                ActiveClasses = activeClasses,
                InactiveClasses = inactiveClasses,
                TotalTeachers = totalTeachers,
                TotalStudents = totalStudents,
                TotalClasses = totalClasses,
                TotalExams = totalExams,
                TotalQuestions = totalQuestions,
                TotalSubjects = totalSubjects
            };
            return View(vm);
        }

    }
}
