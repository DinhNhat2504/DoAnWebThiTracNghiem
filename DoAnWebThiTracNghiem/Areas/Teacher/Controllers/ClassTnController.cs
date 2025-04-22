using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoAnWebThiTracNghiem.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    public class ClassTnController : Controller
    {
        private readonly IClassTnRepository _classTnRepository;
        private readonly AppDBContext _context;
        public ClassTnController(IClassTnRepository classTnRepository, AppDBContext context)
        {
            _classTnRepository = classTnRepository;
            _context = context;
        }
        // Hiển thị danh sách lớp học do Teacher có id = session tạo 
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);
            var classTn = await _classTnRepository.GetAllAsync(teacherId);
            return View(classTn);
        }
        // Tạo lớp học mới
        public IActionResult Create()
        {

            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Subject_Id", "Subject_Name");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(ClassTn classTn)
        {
            if (ModelState.IsValid)
            {
                var userID = HttpContext.Session.GetString("UserId");
                classTn.CreatorUser_Id = int.Parse(userID);
                classTn.CreatedAt = DateTime.Now;
                classTn.UpdatedAt = DateTime.Now;
                classTn.InviteCode = await GenerateUniqueInviteCode();
                await _classTnRepository.AddAsync(classTn);
                return RedirectToAction(nameof(Index));
            }
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Subject_Id", "Subject_Name", classTn.SubjectId);
            return View(classTn);
        }
        // Chỉnh sửa lớp học
        public async Task<IActionResult> Edit(int id)
        {
            var classTn = await _classTnRepository.GetByIdAsync(id);
            if (classTn == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetString("UserId");
            if (classTn.CreatorUser_Id != int.Parse(userId))
            {
                return Unauthorized();
            }

            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Subject_Id", "Subject_Name", classTn.SubjectId);
            return View(classTn);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, ClassTn classTn)
        {
            if (id != classTn.Class_Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetString("UserId");
                var existingClass = await _classTnRepository.GetByIdAsync(id);
                if (existingClass == null)
                {
                    return NotFound();
                }

                // Chỉ cập nhật các trường được phép
                existingClass.ClassName = classTn.ClassName;
                existingClass.CreatorUser_Id = int.Parse(userId);
                existingClass.SubjectId = classTn.SubjectId;
                existingClass.IsActive = classTn.IsActive;
                existingClass.UpdatedAt = DateTime.Now;
                await _classTnRepository.UpdateAsync(existingClass);
                return RedirectToAction(nameof(Index));
            }
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Subject_Id", "Subject_Name", classTn.SubjectId);
            return View(classTn);
        }
        // Xóa lớp học
        public async Task<IActionResult> Delete(int id)
        {
            var classTn = await _classTnRepository.GetByIdAsync(id);
            if (classTn == null)
            {
                return NotFound();
            }
            var userId = HttpContext.Session.GetString("UserId");
            if (classTn.CreatorUser_Id != int.Parse(userId))
            {
                return Unauthorized();
            }
            return View(classTn);
        }
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            
            await _classTnRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
        private async Task<string> GenerateUniqueInviteCode()
        {
            string inviteCode;
            bool exists;

            do
            {
                // Tạo mã InviteCode gồm 7 ký tự chữ hoa và số
                inviteCode = GenerateRandomCode(7);

                // Kiểm tra xem mã này đã tồn tại trong cơ sở dữ liệu chưa
                exists = await _context.ClassTn.AnyAsync(c => c.InviteCode == inviteCode);
            } while (exists);

            return inviteCode;
        }

        private string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}
