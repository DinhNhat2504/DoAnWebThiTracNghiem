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
        private readonly ISubjectRepository _Scontext;

        public ExamsController(IExamRepository econtext, ISubjectRepository scontext)
        {
            _Econtext = econtext;
            _Scontext = scontext;
        }
        public async Task<IActionResult> Index( int page = 1, int pageSize = 10 , string search="" )
        {
            var userId = HttpContext.Session.GetString("RoleId");
            int adminId = int.Parse(userId);
            var totalExams = await _Econtext.CountAsync(1,adminId,search);
            var exams = await _Econtext.GetPagedAsync(1,adminId, page, pageSize, search);
            var subjects = await _Scontext.GetAllAsync();
            var model = new PagedResult<Exam>
            {
                Items = exams,
                PageNumber = page,
                TotalPages = (int)Math.Ceiling(totalExams / (double)pageSize),
                TotalItems = totalExams,
                Search = search
            };
            ViewData["Subjects"] = subjects; // Lưu danh sách môn học vào ViewData để sử dụng trong View
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Exam exam)
        {
            ModelState.Remove("EndTime");
            var userId = HttpContext.Session.GetString("UserId");
            int AdminId = int.Parse(userId);
            if (ModelState.IsValid)
            {
                exam.StartTime = DateTime.Now;
                exam.EndTime = DateTime.Now;
                exam.CreateAt = DateTime.Now;
                exam.CreatorUser_Id = AdminId;
                await _Econtext.AddAsync(exam);
                // Trả về JSON thành công
                return Json(new { success = true, message = "Thêm bài thi thành công !" });
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
            var existing = await _Econtext.GetByIdAsync(exam.Exam_ID);
            if (existing == null) return NotFound();

            existing.Exam_Name = exam.Exam_Name;
            existing.TotalQuestions = exam.TotalQuestions;
            existing.Duration = exam.Duration;
            existing.PassScore = exam.PassScore;
            existing.Exam_Date = exam.Exam_Date;
            
            existing.Subject_ID = exam.Subject_ID;
            existing.StartTime = exam.StartTime;
            existing.EndTime = exam.EndTime;

            await _Econtext.UpdateAsync(existing);
            return Json(new { success = true, message = "Sửa bài thi thành công!" });
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

        // DoAnWebThiTracNghiem\Areas\Admin\Controllers\ExamsController.cs
        public async Task<IActionResult> Details(int id)
        {
            var exam = await _Econtext.GetByIdQSAsync(id);
            if (exam == null) return NotFound();

            // Lấy danh sách câu hỏi thuộc bài thi này (đã bao gồm thông tin loại câu hỏi)
            var questions = exam.Exam_Questions?
                .OrderBy(q => q.Question_Order)
                .Select(q => q.Question)
                .ToList();

            ViewData["Questions"] = questions;
            return View(exam);
        }


    }
      
}
