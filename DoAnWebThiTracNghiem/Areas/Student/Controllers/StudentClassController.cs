using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnWebThiTracNghiem.Areas.Student.Controllers
{
    [Area("Student")]
    public class StudentClassController : Controller
    {
        private readonly AppDBContext _context;
        public StudentClassController(AppDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Users");
            }

            int studentId = int.Parse(userId);

            // Lấy danh sách lớp học mà học sinh đã tham gia
            var joinedClasses = await _context.ClassStudents
         .Where(sc => sc.User_ID == studentId)
         .Include(sc => sc.ClassTn) // Include thông tin lớp học
             .ThenInclude(c => c.Subject) // Include thông tin môn học
         .Include(sc => sc.ClassTn) // Include thông tin lớp học
             .ThenInclude(c => c.Creator) // Include thông tin người tạo
         .Select(sc => sc.ClassTn)
         .ToListAsync();

            return View(joinedClasses);
        }
        [HttpPost]
        public async Task<IActionResult> JoinClass(string inviteCode)
        {
            var classTn = await _context.ClassTn.FirstOrDefaultAsync(c => c.InviteCode == inviteCode);
            if (classTn == null)
            {
                ModelState.AddModelError(string.Empty, "Mã lớp học không hợp lệ.");
                return RedirectToAction("Index");
            }

            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Users");
            }

            int studentId = int.Parse(userId);

            // Kiểm tra xem học sinh đã tham gia lớp học chưa
            var existingEntry = await _context.ClassStudents
                .FirstOrDefaultAsync(sc => sc.User_ID == studentId && sc.Class_ID == classTn.Class_Id);

            if (existingEntry != null)
            {
                TempData["ErrorMessage"] = "Bạn đã tham gia lớp học này.";
                return RedirectToAction("Index");
            }

            // Thêm học sinh vào lớp học
            var studentClass = new Student_Class
            {
                User_ID = studentId,
                Class_ID = classTn.Class_Id,
                Timestamp = DateTime.Now // Lưu thời gian tham gia
            };

            _context.ClassStudents.Add(studentClass);

            // Nếu lớp học đang ở trạng thái không hoạt động, chuyển sang hoạt động
            if (!classTn.IsActive)
            {
                classTn.IsActive = true;
                classTn.UpdatedAt = DateTime.Now;
                _context.ClassTn.Update(classTn);
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "Bạn đã tham gia lớp học thành công.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int id)
        {
            var classTn = await _context.ClassTn
                .Include(c => c.Subject)
                .Include(c => c.Creator)
                .FirstOrDefaultAsync(c => c.Class_Id == id);

            if (classTn == null)
            {
                return NotFound();
            }

            var notifications = await _context.Notifications
                .Where(n => n.ClassTNClass_Id == id)
                .OrderByDescending(n => n.Timestamp)
                .ToListAsync();

            var exams = await _context.ClassExams
                .Include(ec => ec.Exam)
                .Where(ec => ec.ClassTNClass_Id == id)
                .OrderByDescending(ec => ec.AssignedAt)
                .ToListAsync();

            // Chuyển đổi giờ về giờ Việt Nam cho từng bài thi
            foreach (var ec in exams)
            {
                if (ec.Exam != null)
                {
                    ec.Exam.StartTime = ToVietnamTime(ec.Exam.StartTime);
                    ec.Exam.EndTime = ToVietnamTime(ec.Exam.EndTime);
                }
            }

            var userId = HttpContext.Session.GetString("UserId");
            int studentId = int.Parse(userId);

            var examResults = await _context.ExamResult
                .Where(er => er.User_ID == studentId)
                .ToListAsync();

            ViewData["Class"] = classTn;
            ViewData["Notifications"] = notifications;
            ViewData["Exams"] = exams;
            ViewData["ExamResults"] = examResults;
            ViewData["UserId"] = userId;

            return View();
        }

        // Thêm hàm này vào controller
        private DateTime ToVietnamTime(DateTime utcDateTime)
        {
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var utc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, vnTimeZone);
        }

        [HttpPost]
        public async Task<IActionResult> LeaveClass(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Users");
            }

            int studentId = int.Parse(userId);

            // Tìm bản ghi trong bảng ClassStudents
            var studentClass = await _context.ClassStudents
                .FirstOrDefaultAsync(sc => sc.User_ID == studentId && sc.Class_ID == id);

            if (studentClass != null)
            {
                _context.ClassStudents.Remove(studentClass); // Xóa bản ghi
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "StudentClass");
        }
    }
}
