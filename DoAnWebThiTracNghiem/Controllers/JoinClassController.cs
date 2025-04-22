using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnWebThiTracNghiem.Controllers
{
    public class JoinClassController : Controller
    {
        private readonly AppDBContext _context;

        public JoinClassController(AppDBContext context)
        {
            _context = context;
        }

        // Hiển thị form nhập InviteCode
        public IActionResult Join()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Join(string inviteCode)
        {
            var classTn = await _context.ClassTn.FirstOrDefaultAsync(c => c.InviteCode == inviteCode);
            if (classTn == null)
            {
                ModelState.AddModelError(string.Empty, "Mã lớp học không hợp lệ.");
                return View();
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
                return RedirectToAction("Join", "JoinClass");
            }

            // Thêm học sinh vào lớp học
            var studentClass = new Student_Class
            {
                User_ID = studentId,
                Class_ID = classTn.Class_Id
            };

            _context.ClassStudents.Add(studentClass);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Bạn đã tham gia lớp học thành công.";
            return RedirectToAction("Join", "JoinClass");
        }

    }
}

