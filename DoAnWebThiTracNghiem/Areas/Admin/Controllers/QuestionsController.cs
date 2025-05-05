using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWebThiTracNghiem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuestionsController : Controller
    {
        public readonly IQuestionRepository _Qcontext;
        public QuestionsController(IQuestionRepository qcontext)
        {
            _Qcontext = qcontext;
        }
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            int adminId = int.Parse(userId);
            var questions = await _Qcontext.GetAllAsync(adminId);
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
        public async Task<IActionResult> Create(Question question)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetString("UserId");
                int adminId = int.Parse(userId);
                question.CreatorUser_Id = adminId;
                await _Qcontext.AddAsync(question);
                return RedirectToAction(nameof(Index));
            }
            return View(question);
        }
        
    }
}
