using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.Repositories;
using DoAnWebThiTracNghiem.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoAnWebThiTracNghiem.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    public class SubjectsController : Controller
    {
        private readonly ISubjectRepository _subjectRepository;
        private readonly AppDBContext _context;
        public SubjectsController(ISubjectRepository subjectRepository, AppDBContext context)
        {
            _subjectRepository = subjectRepository;
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 5, string search = "")
        {
            var UserId = HttpContext.Session.GetString("UserId");
            var RoleId = HttpContext.Session.GetString("RoleId");
            int userId = int.Parse(UserId);
            int roleId = int.Parse(RoleId);
            var total = await _subjectRepository.CountAsync(roleId, userId, search);
            var items = await _subjectRepository.GetPagedAsync(roleId, userId, page, pageSize, search);

            var model = new PagedResult<Subject>
            {
                Items = items,
                PageNumber = page,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                TotalItems = total,
                Search = search
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Subject subject)
        {
            var userID = HttpContext.Session.GetString("UserId");
            int userId = int.Parse(userID);

            if (ModelState.IsValid)
            {
                var existingSubject = await _subjectRepository.GetAllAsync(2, userId);
                if (existingSubject.Any(u => u.Subject_Name == subject.Subject_Name))
                {

                    TempData["SErrorMessage"] = "Tên môn học này đã tồn tại";
                    return RedirectToAction(nameof(Index));

                }

                subject.CreatorUser_Id = userId;
                await _subjectRepository.AddAsync(subject);
                TempData["SSuccessMessage"] = "Thêm môn học mới thành công !";
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var subject = await _context.Subjects
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Subject_Id == id);

            if (subject == null)
                return NotFound();

            return Json(new
            {
                subject_Id = subject.Subject_Id,
                subject_Name = subject.Subject_Name
            });
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Subject subject)
        {
            try
            {
                if (subject == null) return BadRequest();
                var existing = await _subjectRepository.GetByIdAsync(subject.Subject_Id);
                if (existing == null) return NotFound();

                var userID = HttpContext.Session.GetString("UserId");

                subject.CreatorUser_Id = int.Parse(userID);
                existing.Subject_Name = subject.Subject_Name;


                await _subjectRepository.UpdateAsync(existing);

                return Ok();


            }
            catch (Exception)
            {
                return BadRequest();
            }


        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var classTn = await _subjectRepository.GetByIdAsync(id);
                if (classTn == null)
                {
                    TempData["SErrorMessage"] = "Không tìm thấy môn học để xóa.";
                    return RedirectToAction(nameof(Index));
                }

                await _subjectRepository.DeleteAsync(id);
                TempData["SSuccessMessage"] = "Xóa môn học thành công.";
            }
            catch (Exception e)
            {
                TempData["SErrorMessage"] = "Có lỗi xảy ra khi xóa môn học.";
                Console.WriteLine(e);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
