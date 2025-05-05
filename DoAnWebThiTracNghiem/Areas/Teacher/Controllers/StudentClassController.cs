using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnWebThiTracNghiem.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    public class StudentClassController : Controller
    {
        private readonly AppDBContext _context;

        public StudentClassController(AppDBContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> ExamResults(int examId, int classId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);

            // Lấy danh sách User_ID của sinh viên trong lớp này
            var studentIdsInClass = await _context.ClassStudents
                .Where(sc => sc.Class_ID == classId)
                .Select(sc => sc.User_ID)
                .ToListAsync();

            // Lọc kết quả bài thi của sinh viên thuộc lớp này
            var results = await _context.ExamResult
                .Where(r => r.Exam_ID == examId
                            && r.Exam.CreatorUser_Id == teacherId
                            && studentIdsInClass.Contains(r.User_ID))
                .Include(r => r.User)
                .Include(r => r.Exam)
                .ToListAsync();

            ViewBag.ExamName = results.FirstOrDefault()?.Exam?.Exam_Name ?? "Exam";
            return View(results);
        }

        // GET: Teacher/StudentClass/ExamResultDetail/5
        public async Task<IActionResult> ExamResultDetail(int resultId)
        {
            var result = await _context.ExamResult
                .Include(r => r.User)
                .Include(r => r.Exam)
                .Include(r => r.Student_Answers)
                    .ThenInclude(sa => sa.Question)
                .FirstOrDefaultAsync(r => r.Result_ID == resultId);

            if (result == null)
            {
                return NotFound();
            }

            return View(result);
        }

    }
}
