using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.Repositories;
using DoAnWebThiTracNghiem.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnWebThiTracNghiem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ExamsController : Controller
    {
        private readonly IExamRepository _Econtext;
        public ExamsController(IExamRepository econtext)
        {
            _Econtext = econtext;
        }
        public async Task<IActionResult> Index( int page = 1, int pageSize = 10)
        {
            var userId = HttpContext.Session.GetString("RoleId");
            int adminId = int.Parse(userId);
            var totalExams = await _Econtext.CountAsync(adminId);
            var exams = await _Econtext.GetPagedAsync(adminId, page, pageSize);

            var model = new PagedResult<Exam>
            {
                Items = exams,
                PageNumber = page,
                TotalPages = (int)Math.Ceiling(totalExams / (double)pageSize),
                TotalItems = totalExams
            };
            return View(model);
        }

        // API lấy thông tin exam
        [HttpGet]
        public async Task<IActionResult> GetExam(int id)
        {
            var exam = await _Econtext.GetByIdAsync(id);
            if (exam == null) return NotFound();
            return Json(exam);
        }

        // API sửa exam
        [HttpPost]
        public async Task<IActionResult> EditInline(Exam exam)
        {
            if (exam == null) return BadRequest();
            var existing = await _Econtext.GetByIdAsync(exam.Exam_ID);
            if (existing == null) return NotFound();

            existing.Exam_Name = exam.Exam_Name;
            existing.TotalQuestions = exam.TotalQuestions;
            existing.Duration = exam.Duration;
            existing.PassScore = exam.PassScore;
            existing.Exam_Date = exam.Exam_Date;
            existing.IsActive = exam.IsActive;
            existing.Subject_ID = exam.Subject_ID;
            existing.StartTime = exam.StartTime;
            existing.EndTime = exam.EndTime;

            await _Econtext.UpdateAsync(existing);
            return Ok();
        }

        // API xóa exam
        [HttpPost]
        public async Task<IActionResult> DeleteInline(int id)
        {
            var exam = await _Econtext.GetByIdAsync(id);
            if (exam == null) return NotFound();
            await _Econtext.DeleteAsync(id);
            return Ok();
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
        public async Task<IActionResult> Create(Exam exam)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetString("UserId");
                int adminId = int.Parse(userId);
                exam.CreatorUser_Id = adminId;
                await _Econtext.AddAsync(exam);
                return RedirectToAction(nameof(Index));
            }
            return View(exam);
        }
    }
      
}
