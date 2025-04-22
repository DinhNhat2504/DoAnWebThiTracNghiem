using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.Repositories;
using Microsoft.AspNetCore.Mvc;

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

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);
            var subject = await _subjectRepository.GetAllAsync(teacherId);
            return View(subject);
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

        public async Task<IActionResult> Edit(int id)
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
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Subject subject)
        {
            if (id != subject.Subject_Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingSubject = await _subjectRepository.GetByIdAsync(id);
                var userID = HttpContext.Session.GetString("UserId");
                existingSubject.Subject_Name = subject.Subject_Name;
                subject.CreatorUser_Id = int.Parse(userID);
                existingSubject.CreatorUser_Id = subject.CreatorUser_Id;
                await _subjectRepository.UpdateAsync(existingSubject);
                return RedirectToAction(nameof(Index));
            }
            return View(subject);
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
            var subject = await _context.Subjects.FindAsync(id);
            if (subject != null)
            {
                _context.Subjects.Remove(subject);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
