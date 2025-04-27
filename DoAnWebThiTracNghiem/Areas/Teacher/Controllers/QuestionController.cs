using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DoAnWebThiTracNghiem.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    public class QuestionController : Controller
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly AppDBContext _context;
        public QuestionController(IQuestionRepository questionRepository, AppDBContext context)
        {
            _questionRepository = questionRepository;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);
            var question = await _questionRepository.GetAllAsync(teacherId);
            return View(question);
        }
        public async Task<IActionResult> Create()
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);
            ViewData["SubjectId"] = new SelectList(_context.Subjects.Where(s => s.CreatorUser_Id == teacherId), "Subject_Id", "Subject_Name");
            ViewData["LevelId"] = new SelectList(_context.Levels, "Id", "LevelName");

            return View(new Question());
        }
        [HttpPost]
        public async Task<IActionResult> Create(Question question)
        {
            if (ModelState.IsValid)
            {
                var userID = HttpContext.Session.GetString("UserId");
                question.CreatorUser_Id = int.Parse(userID);
                question.CreatedAt = DateTime.Now;
                await _questionRepository.AddAsync(question);
                return RedirectToAction(nameof(Index));
            }
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Subject_Id", "Subject_Name", question.Subject_ID);
            ViewData["LevelId"] = new SelectList(_context.Levels, "Level_ID", "Level_Name", question.Level_ID);
            return View(question);
        }
        public
            async Task<IActionResult> Edit(int id)
        {
            var question = await _questionRepository.GetByIdAsync(id);
            if (question == null)
            {
                return NotFound();
            }
            var userId = HttpContext.Session.GetString("UserId");
            var teacherId = int.Parse(userId);
            if (question.CreatorUser_Id != teacherId)
            {
                return Unauthorized();
            }
            ViewData["SubjectId"] = new SelectList(_context.Subjects.Where(s => s.CreatorUser_Id == teacherId), "Subject_Id", "Subject_Name");
            ViewData["LevelId"] = new SelectList(_context.Levels, "Level_ID", "Level_Name", question.Level_ID);
            return View(question);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Question question)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetString("UserId");
                question.CreatorUser_Id = int.Parse(userId);
                question.CreatedAt = DateTime.Now;
                await _questionRepository.UpdateAsync(question);
                return RedirectToAction(nameof(Index));
            }
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Subject_Id", "Subject_Name", question.Subject_ID);
            ViewData["LevelId"] = new SelectList(_context.Levels, "Level_ID", "Level_Name", question.Level_ID);
            return View(question);
        }

    }
}
