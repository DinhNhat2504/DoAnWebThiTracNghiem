using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.Repositories;
using DoAnWebThiTracNghiem.ViewModel;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Subject subject)
        {
            if (ModelState.IsValid)
            {
                var userID = HttpContext.Session.GetString("UserId");
                subject.CreatorUser_Id = int.Parse(userID);
                await _subjectRepository.AddAsync(subject);
                return RedirectToAction(nameof(Index));
            }
            return View(subject);
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
        public async Task<IActionResult> Edit( Subject subject)
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
        public async Task<IActionResult> Delete(int id)
        {
            var subject = await _subjectRepository.GetByIdAsync(id);
            var userId = HttpContext.Session.GetString("UserId");
            if (subject.CreatorUser_Id != int.Parse(userId))
            {
                return Unauthorized();
            }
            if (subject == null)
            {
                return NotFound();
            }
            return View(subject);
        }
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var classTn = await _subjectRepository.GetByIdAsync(id);
                if (classTn == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy lớp học để xóa.";
                    return RedirectToAction(nameof(Index));
                }

                await _subjectRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = "Xóa lớp học thành công.";
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa lớp học.";
                Console.WriteLine(e);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
