using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.Repositories;
using DoAnWebThiTracNghiem.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Drawing.Printing;

namespace DoAnWebThiTracNghiem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ClassTnController : Controller
    {
        private readonly IClassTnRepository _Clcontext;
        public ClassTnController(IClassTnRepository clcontext)
        {
            _Clcontext = clcontext;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 5, string search = "")
        {
            var UserId = HttpContext.Session.GetString("UserId");
            var RoleId = HttpContext.Session.GetString("RoleId");
            int userId = int.Parse(UserId);
            int roleId = int.Parse(RoleId);
            var total = await _Clcontext.CountAsync(roleId,userId,search);
            var items = await _Clcontext.GetPagedAsync(roleId, userId,page, pageSize, search);

            var model = new PagedResult<ClassTn>
            {
                Items = items,
                PageNumber = page,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                TotalItems = total,
                Search = search
            };
            
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> GetSubjectsForClass(int classId)
        {
            var classTn = await _Clcontext.GetByIdAsync(classId);
            if (classTn == null)
                return Json(new List<Subject>());
            var subjects = await _Clcontext.GetSubjectsByCreatorAsync(classTn.CreatorUser_Id);
            return Json(subjects ?? new List<Subject>());
        }


        public IActionResult Details()
        {
            return View();
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassTn classTn)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetString("UserId");
                int adminId = int.Parse(userId);
                classTn.CreatorUser_Id = adminId;
                await _Clcontext.AddAsync(classTn);
                return RedirectToAction(nameof(Index));
            }
            return View(classTn);
        }
        // API lấy thông tin lớp học
        [HttpGet]
        public async Task<IActionResult> GetClass(int id)
        {
            var classTn = await _Clcontext.GetByIdAsync(id);
            if (classTn == null) return NotFound();
            return Json(classTn);
        }

        // API sửa lớp học
        [HttpPost]
        public async Task<IActionResult> EditInline(ClassTn classTn)
        {
            if (classTn == null) return BadRequest();
            var existing = await _Clcontext.GetByIdAsync(classTn.Class_Id);
            if (existing == null) return NotFound();

            existing.ClassName = classTn.ClassName;
            existing.SubjectId = classTn.SubjectId;
            existing.UpdatedAt = DateTime.Now;
            await _Clcontext.UpdateAsync(existing);
            return Ok();
        }

        // API xóa lớp học
        [HttpPost]
        public async Task<IActionResult> DeleteInline(int id)
        {
            var classTn = await _Clcontext.GetByIdAsync(id);
            if (classTn == null) return NotFound();
            await _Clcontext.DeleteAsync(id);
            return Ok();
        }
    }
}
