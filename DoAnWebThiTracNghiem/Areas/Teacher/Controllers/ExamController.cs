using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoAnWebThiTracNghiem.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    public class ExamController : Controller
    {
        private readonly IExamRepository _examRepository;
        private readonly AppDBContext _context;
        public ExamController(IExamRepository examRepository, AppDBContext context)
        {
            _examRepository = examRepository;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);
            var exam = await _examRepository.GetAllAsync(teacherId);
            return View(exam);
        }
        public async Task<IActionResult> Create()
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);
            ViewData["SubjectId"] = new SelectList(_context.Subjects.Where(s => s.CreatorUser_Id == teacherId), "Subject_Id", "Subject_Name");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Exam exam)
        {
            if (ModelState.IsValid)
            {
                var userID = HttpContext.Session.GetString("UserId");
                exam.CreatorUser_Id = int.Parse(userID);
                exam.Exam_Date = DateTime.Now;
                await _examRepository.AddAsync(exam);
                return RedirectToAction(nameof(Index));
            }
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Subject_Id", "Subject_Name", exam.Subject_ID);
            return View(exam);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var exam = await _examRepository.GetByIdAsync(id);
            if (exam == null)
            {
                return NotFound();
            }
            var userId = HttpContext.Session.GetString("UserId");
            var teacherId = int.Parse(userId);
            if (exam.CreatorUser_Id != teacherId)
            {
                return Unauthorized();
            }
            ViewData["SubjectId"] = new SelectList(_context.Subjects.Where(s => s.CreatorUser_Id == teacherId), "Subject_Id", "Subject_Name");
            return View(exam);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Exam exam)
        {
            if (id != exam.Exam_ID)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetString("UserId");
                exam.CreatorUser_Id = int.Parse(userId);
                await _examRepository.UpdateAsync(exam);
                return RedirectToAction(nameof(Index));
            }
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Subject_Id", "Subject_Name", exam.Subject_ID);
            return View(exam);
        }
        public async Task<IActionResult> Delete(int id)
        {
            var exam = await _examRepository.GetByIdAsync(id);
            if (exam == null)
            {
                return NotFound();
            }
            var userId = HttpContext.Session.GetString("UserId");
            var teacherId = int.Parse(userId);
            if (exam.CreatorUser_Id != teacherId)
            {
                return Unauthorized();
            }
            return View(exam);
        }
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var examQuestions = _context.ExamQuestions.Where(eq => eq.Exam_ID == id);
            _context.ExamQuestions.RemoveRange(examQuestions);
            await _examRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> AddQuestions(int examId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);

            // Kiểm tra xem giáo viên có quyền thêm câu hỏi vào đề thi không
            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Exam_ID == examId && e.CreatorUser_Id == teacherId);
            if (exam == null)
            {
                return Unauthorized();
            }

            // Lấy danh sách ID các câu hỏi đã có trong bài thi
            var existingQuestionIds = await _context.ExamQuestions
                .Where(eq => eq.Exam_ID == examId)
                .Select(eq => eq.Question_ID)
                .ToListAsync();

            // Lấy danh sách câu hỏi do giáo viên tạo, loại bỏ các câu hỏi đã có trong bài thi
            var questions = await _context.Question
       .Where(q => q.CreatorUser_Id == teacherId
                   && q.Subject_ID == exam.Subject_ID
                   && !existingQuestionIds.Contains(q.Question_ID))
       .ToListAsync();

            ViewData["ExamId"] = examId;
            ViewData["TotalQuestions"] = exam.TotalQuestions;

            return View(questions);
        }

        [HttpPost]
        public async Task<IActionResult> AddQuestions(int examId, List<int> questionIds)
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);

            // Kiểm tra xem giáo viên có quyền thêm câu hỏi vào đề thi không
            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Exam_ID == examId && e.CreatorUser_Id == teacherId);
            if (exam == null)
            {
                return Unauthorized();
            }

            // Thêm câu hỏi mới vào bài thi
            int currentOrder = await _context.ExamQuestions
                .Where(eq => eq.Exam_ID == examId)
                .MaxAsync(eq => (int?)eq.Question_Order) ?? 0;

            foreach (var questionId in questionIds)
            {
                currentOrder++; // Tăng thứ tự
                var examQuestion = new Exam_Question
                {
                    Exam_ID = examId,
                    Question_ID = questionId,
                    Question_Order = currentOrder, // Gán thứ tự cho câu hỏi
                    Points = 0 // Tạm thời gán 0, sẽ tính lại sau
                };
                _context.ExamQuestions.Add(examQuestion);
            }

            await _context.SaveChangesAsync();

            // Lấy danh sách tất cả các câu hỏi trong bài thi
            var allQuestions = await _context.ExamQuestions
                .Where(eq => eq.Exam_ID == examId)
                .OrderBy(eq => eq.Question_Order)
                .ToListAsync();

            // Tính lại điểm cho tất cả các câu hỏi
            int totalQuestions = allQuestions.Count;
            decimal pointsPerQuestion = totalQuestions > 0 ? 10m / totalQuestions : 0;

            int newOrder = 1;
            foreach (var question in allQuestions)
            {
                question.Question_Order = newOrder++;
                question.Points = pointsPerQuestion;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thêm câu hỏi thành công và điểm, thứ tự đã được cập nhật.";
            return RedirectToAction("AddQuestions", new { examId });
        }


        [HttpGet]
        public async Task<IActionResult> ViewQuestions(int examId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);

            // Kiểm tra xem giáo viên có quyền xem câu hỏi trong đề thi không
            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Exam_ID == examId && e.CreatorUser_Id == teacherId);
            if (exam == null)
            {
                return Unauthorized();
            }

            // Lấy danh sách câu hỏi đã thêm vào đề thi (Exam_Question)
            var examQuestions = await _context.ExamQuestions
                .Where(eq => eq.Exam_ID == examId)
                .Include(eq => eq.Question) // Bao gồm thông tin câu hỏi
                .OrderBy(eq => eq.Question_Order) // Sắp xếp theo thứ tự câu hỏi
                .ToListAsync();

            ViewData["ExamId"] = examId;

            return View(examQuestions); // Truyền danh sách Exam_Question vào view
        }

        public async Task<IActionResult> RemoveQuestion(int examId, int questionId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);

            // Kiểm tra xem giáo viên có quyền xóa câu hỏi khỏi bài thi không
            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Exam_ID == examId && e.CreatorUser_Id == teacherId);
            if (exam == null)
            {
                return Unauthorized();
            }

            // Tìm câu hỏi trong bài thi
            var examQuestion = await _context.ExamQuestions
                .FirstOrDefaultAsync(eq => eq.Exam_ID == examId && eq.Question_ID == questionId);

            if (examQuestion == null)
            {
                return NotFound();
            }

            // Xóa câu hỏi khỏi bài thi
            _context.ExamQuestions.Remove(examQuestion);
            await _context.SaveChangesAsync();

            // Lấy danh sách các câu hỏi còn lại trong bài thi, sắp xếp theo Question_Order
            var remainingQuestions = await _context.ExamQuestions
                .Where(eq => eq.Exam_ID == examId)
                .OrderBy(eq => eq.Question_Order)
                .ToListAsync();

            // Tính lại điểm cho mỗi câu hỏi
            int totalQuestions = remainingQuestions.Count;
            decimal pointsPerQuestion = totalQuestions > 0 ? 10m / totalQuestions : 0;

            // Cập nhật thứ tự và điểm số cho các câu hỏi còn lại
            int newOrder = 1;
            foreach (var question in remainingQuestions)
            {
                question.Question_Order = newOrder++;
                question.Points = pointsPerQuestion;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Câu hỏi đã được xóa khỏi bài thi và thứ tự, điểm số đã được cập nhật.";
            return RedirectToAction("ViewQuestions", new { examId });
        }

        [HttpGet]
        public async Task<IActionResult> CreateWithQuestions()
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);

            // Lấy danh sách môn học do giáo viên tạo
            ViewData["SubjectId"] = new SelectList(_context.Subjects.Where(s => s.CreatorUser_Id == teacherId), "Subject_Id", "Subject_Name");

            // Lấy danh sách câu hỏi do giáo viên tạo
            var questions = await _context.Question
                .Where(q => q.CreatorUser_Id == teacherId)
                .ToListAsync();

            ViewData["Questions"] = questions;

            return View(new Exam());
        }
        [HttpPost]
        public async Task<IActionResult> CreateWithQuestions(Exam exam, List<int> questionIds)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetString("UserId");
                exam.CreatorUser_Id = int.Parse(userId);
                exam.Exam_Date = DateTime.Now;

                // Thêm bài thi vào cơ sở dữ liệu
                _context.Exams.Add(exam);
                await _context.SaveChangesAsync();

                // Tính điểm cho mỗi câu hỏi
                decimal pointsPerQuestion = 10m / questionIds.Count;

                // Thêm các câu hỏi vào bài thi
                int currentOrder = 0;
                foreach (var questionId in questionIds)
                {
                    currentOrder++; // Tăng thứ tự
                    var examQuestion = new Exam_Question
                    {
                        Exam_ID = exam.Exam_ID,
                        Question_ID = questionId,
                        Question_Order = currentOrder, // Gán thứ tự cho câu hỏi
                        Points = pointsPerQuestion // Gán điểm cho câu hỏi
                    };
                    _context.ExamQuestions.Add(examQuestion);
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Tạo bài thi và thêm câu hỏi thành công.";
                return RedirectToAction(nameof(Index));
            }

            // Nếu có lỗi, hiển thị lại form
            var teacherId = int.Parse(HttpContext.Session.GetString("UserId"));
            ViewData["SubjectId"] = new SelectList(_context.Subjects.Where(s => s.CreatorUser_Id == teacherId), "Subject_Id", "Subject_Name");
            ViewData["Questions"] = await _context.Question.Where(q => q.CreatorUser_Id == teacherId).ToListAsync();

            return View(exam);
        }

        [HttpGet]
        public async Task<IActionResult> AssignExam(int examId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);

            // Kiểm tra xem giáo viên có quyền giao bài thi không
            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Exam_ID == examId && e.CreatorUser_Id == teacherId);
            if (exam == null)
            {
                return Unauthorized();
            }

            // Lấy danh sách lớp học do giáo viên tạo
            var classes = await _context.ClassTn
                .Where(c => c.CreatorUser_Id == teacherId)
                .ToListAsync();

            ViewData["ExamId"] = examId;
            ViewData["ExamName"] = exam.Exam_Name;

            return View(classes);
        }
        [HttpPost]
        public async Task<IActionResult> AssignExam(int examId, int classId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);

            // Kiểm tra xem giáo viên có quyền giao bài thi không
            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Exam_ID == examId && e.CreatorUser_Id == teacherId);
            if (exam == null)
            {
                return Unauthorized();
            }

            // Kiểm tra xem lớp học có thuộc giáo viên không
            var classTn = await _context.ClassTn.FirstOrDefaultAsync(c => c.Class_Id == classId && c.CreatorUser_Id == teacherId);
            if (classTn == null)
            {
                return Unauthorized();
            }

            // Kiểm tra xem bài thi đã được giao cho lớp học chưa
            var existingAssignment = await _context.ClassExams
                .FirstOrDefaultAsync(ce => ce.Exam_ID == examId && ce.ClassTNClass_Id == classId);

            if (existingAssignment != null)
            {
                TempData["ErrorMessage"] = "Bài thi đã được giao cho lớp học này.";
                return RedirectToAction("AssignExam", new { examId });
            }

            // Giao bài thi cho lớp học
            var classExam = new Exam_Class
            {
                Exam_ID = examId,
                ClassTNClass_Id = classId,
                AssignedAt = DateTime.Now
            };
            _context.ClassExams.Add(classExam);

            // Cập nhật trạng thái IsActive của bài thi
            exam.IsActive = true;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bài thi đã được giao thành công.";
            return RedirectToAction(nameof(Index));
        }


    }
}
