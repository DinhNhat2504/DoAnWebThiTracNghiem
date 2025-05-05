using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.Repositories;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var adminId = int.Parse(userId);
            var subjects = await _Scontext.GetAllAsync(adminId);
            return View();
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
        public async Task<IActionResult> Create(Subject subject)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetString("UserId");
                int adminId = int.Parse(userId);
                subject.CreatorUser_Id = adminId;
                await _Scontext.AddAsync(subject);
                return RedirectToAction(nameof(Index));
            }
            return View(subject);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var subject = await _Scontext.GetByIdAsync(id);
            if (subject == null)
            {
                return NotFound();
            }
            return View(subject);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Subject subject)
        {
            if (id != subject.Subject_Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                await _Scontext.UpdateAsync(subject);
                return RedirectToAction(nameof(Index));
            }
            return View(subject);
        }
        public IActionResult Delete(int id)
        {
            var subject = _Scontext.GetByIdAsync(id);
            if (subject == null)
            {
                return NotFound();
            }
            return View(subject);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subject = await _Scontext.GetByIdAsync(id);
            if (subject == null)
            {
                return NotFound();
            }
            await _Scontext.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
