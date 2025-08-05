using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnWebThiTracNghiem.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    public class StatisticsController : Controller
    {
        private readonly AppDBContext _context;

        public StatisticsController(AppDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? selectedClassId)
        {
            // Lấy UserId của giáo viên hiện tại
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);

            // Lấy danh sách lớp học do giáo viên tạo
            var classes = await _context.ClassTn
                .Where(c => c.CreatorUser_Id == teacherId)
                .ToListAsync();



            List<StudentStatistics> top10Students = new List<StudentStatistics>();

            if (selectedClassId.HasValue)
            {
                // Lấy danh sách sinh viên của lớp
                var studentIds = await _context.ClassStudents
                    .Where(cs => cs.Class_ID == selectedClassId.Value)
                    .Select(cs => cs.User_ID)
                    .ToListAsync();

                // Lấy danh sách bài thi của lớp
                var examIds = await _context.ClassExams
                    .Where(ec => ec.ClassTNClass_Id == selectedClassId.Value)
                    .Select(ec => ec.Exam_ID)
                    .ToListAsync();

                // Lấy kết quả thi của sinh viên trong lớp với các bài thi của lớp
                var studentAverages = await _context.ExamResult
                    .Where(er => examIds.Contains(er.Exam_ID) && studentIds.Contains(er.User_ID))
                    .GroupBy(er => er.User_ID)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        AverageScore = g.Average(er => er.Score)
                    })
                    .OrderByDescending(g => g.AverageScore)
                    .Take(10)
                    .ToListAsync();

                // Lấy tên sinh viên
                top10Students = studentAverages
                    .Select(g => new StudentStatistics
                    {
                        StudentId = g.UserId,
                        FullName = _context.Users.Where(u => u.User_Id == g.UserId).Select(u => u.FullName).FirstOrDefault(),
                        AverageScore = (double) g.AverageScore
                    })
                    .ToList();
            }

            var viewModel = new StatisticsViewModel
            {
                // Lớp học
                TotalClasses = await _context.ClassTn.CountAsync(c => c.CreatorUser_Id == teacherId),
                ActiveClasses = await _context.ClassTn.CountAsync(c => c.CreatorUser_Id == teacherId && c.IsActive),
                InactiveClasses = await _context.ClassTn.CountAsync(c => c.CreatorUser_Id == teacherId && !c.IsActive),
                ClassWithMostStudents = await _context.ClassTn
                    .Where(c => c.CreatorUser_Id == teacherId)
                    .OrderByDescending(c => c.Student_Classes.Count)
                    .Select(c => c.ClassName)
                    .FirstOrDefaultAsync(),

                // Bài thi
                TotalExams = await _context.Exams.CountAsync(e => e.CreatorUser_Id == teacherId),
                ActiveExams = await _context.Exams.CountAsync(e => e.CreatorUser_Id == teacherId && e.IsActive),
                InactiveExams = await _context.Exams.CountAsync(e => e.CreatorUser_Id == teacherId && !e.IsActive),

                // Câu hỏi
                TotalQuestions = await _context.Question
                    .Where(q => q.CreatorUser_Id == teacherId)
                    .CountAsync(),
                QuestionsByType = await _context.Question
                    .Where(q => q.CreatorUser_Id == teacherId)
                    .GroupBy(q => q.QuestionType.Name)
                    .Select(g => new QuestionTypeStatistics
                    {
                        QuestionTypeName = g.Key,
                        QuestionCount = g.Count()
                    })
                    .ToListAsync(),

                // Môn học
                Subjects = await _context.Subjects
                    .Where(s => s.CreatorUser_Id == teacherId)
                    .Select(s => new SubjectStatistics
                    {
                        SubjectName = s.Subject_Name,
                        ExamCount = s.Exams.Count(e => e.CreatorUser_Id == teacherId),
                        QuestionCount = s.Questions
                            .Where(q => q.CreatorUser_Id == teacherId)
                            .Count()
                    })
                    .ToListAsync(),

                // Sinh viên
                TopStudentByExams = await _context.ExamResult
                    .GroupBy(er => er.User_ID)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        ExamCount = g.Count()
                    })
                    .OrderByDescending(g => g.ExamCount)
                    .Select(g => new StudentStatistics
                    {
                        StudentId = g.UserId,
                        ExamCount = g.ExamCount,
                        FullName = _context.Users.Where(u => u.User_Id == g.UserId).Select(u => u.FullName).FirstOrDefault()
                    })
                    .FirstOrDefaultAsync(),

                TopStudentByAverageScore = await _context.ExamResult
                    .GroupBy(er => er.User_ID)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        AverageScore = g.Average(er => er.Score)
                    })
                    .OrderByDescending(g => g.AverageScore)
                    .Select(g => new StudentStatistics
                    {
                        StudentId = g.UserId,
                        AverageScore = (double)g.AverageScore,
                        FullName = _context.Users.Where(u => u.User_Id == g.UserId).Select(u => u.FullName).FirstOrDefault()
                    })
                    .FirstOrDefaultAsync(),
                Classes = classes,
                SelectedClassId = selectedClassId,
                Top10StudentsByScore = top10Students
            };

            return View(viewModel);
        }
    }
}
