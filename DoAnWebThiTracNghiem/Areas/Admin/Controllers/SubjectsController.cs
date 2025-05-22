using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.Repositories;
using DoAnWebThiTracNghiem.ViewModel;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DoAnWebThiTracNghiem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SubjectsController : Controller
    {
        public readonly ISubjectRepository _Scontext;
        public SubjectsController(ISubjectRepository Scontext)
        {
            _Scontext = Scontext;
        }
        public async Task<IActionResult> Index(int page = 1, int pageSize = 5, string search = "")
        {
            var UserId = HttpContext.Session.GetString("UserId");
            var RoleId = HttpContext.Session.GetString("RoleId");
            int userId = int.Parse(UserId);
            int roleId = int.Parse(RoleId);
            var total = await _Scontext.CountAsync(roleId, userId, search);
            var items = await _Scontext.GetPagedAsync(roleId, userId, page, pageSize, search);

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Subject subject)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetString("UserId");
                int adminId = int.Parse(userId);
                subject.CreatorUser_Id = adminId;
                subject.CreateAt = DateTime.Now;
                await _Scontext.AddAsync(subject);
                return Json(new { success = true, message = "Thêm môn học thành công !" });
            }
            // Trả về lỗi ModelState dạng JSON
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            return BadRequest(new { success = false, errors });
        }
        [HttpGet]
        public async Task<IActionResult> GetSubject(int id)
        {
            var subject = await _Scontext.GetByIdAsync(id);
            if (subject == null) return NotFound();
            return Json(subject);
        }
        [HttpPost]
        public async Task<IActionResult> EditSub(Subject subject)
        {
            if (subject == null) return BadRequest();
            var existing = await _Scontext.GetByIdAsync(subject.Subject_Id);
            if (existing == null) return BadRequest();
            // Trả về lỗi ModelState dạng JSON
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return BadRequest(new { success = false, errors });
            }
            await _Scontext.UpdateAsync(existing);
            return Ok();

        }

        [HttpPost]
        public async Task<IActionResult> DeleteInline(int id)
        {
            var subject = await _Scontext.GetByIdAsync(id);
            if (subject == null)
            {
                return BadRequest(new { success = false });
            }
            await _Scontext.DeleteAsync(id);
            return Ok();
        }
    }
}
