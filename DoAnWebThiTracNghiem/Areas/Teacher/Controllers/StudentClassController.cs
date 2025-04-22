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

        // Hiển thị danh sách sinh viên trong lớp
        public async Task<IActionResult> Index(int classId)
        {
            // Lấy UserId từ session
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Users", new { area = "" });
            }

            int teacherId = int.Parse(userId);

            // Kiểm tra xem giáo viên có phải là người tạo lớp không
            var classTn = await _context.ClassTn.FirstOrDefaultAsync(c => c.Class_Id == classId);
            if (classTn == null)
            {
                return NotFound();
            }

            if (classTn.CreatorUser_Id != teacherId)
            {
                return Unauthorized(); // Hoặc RedirectToAction("AccessDenied", "Home");
            }

            // Lấy danh sách sinh viên trong lớp
            var students = await _context.ClassStudents
                .Where(sc => sc.Class_ID == classId)
                .Include(sc => sc.User)
                .Include(sc => sc.ClassTn)
                .ToListAsync();

            ViewBag.ClassId = classId;
            return View(students);
        }


        // Xóa sinh viên khỏi lớp
        public async Task<IActionResult> Delete(int id)
        {

            var studentClass = await _context.ClassStudents.FindAsync(id);
            if (studentClass == null)
            {
                return NotFound();
            }

            return View(studentClass);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var studentClass = await _context.ClassStudents.FindAsync(id);
            if (studentClass != null)
            {
                _context.ClassStudents.Remove(studentClass);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { classId = studentClass.Class_ID });
        }
    }
}
