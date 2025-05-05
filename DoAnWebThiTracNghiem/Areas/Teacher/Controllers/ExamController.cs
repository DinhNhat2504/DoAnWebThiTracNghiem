using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.Repositories;
using DoAnWebThiTracNghiem.ViewModel;
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
        public async Task<IActionResult> Index(string search)
        {
            // Lấy ID của giáo viên từ session
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);

            // Lấy danh sách bài thi do giáo viên tạo
            var exams = await _context.Exams
                .Include(e => e.Subject) // Bao gồm thông tin môn học
                .Where(e => e.CreatorUser_Id == teacherId) // Lọc theo giáo viên
                .ToListAsync();

            // Lọc theo từ khóa tìm kiếm nếu có
            if (!string.IsNullOrEmpty(search))
            {
                exams = exams.Where(e =>
                    (e.Exam_Name != null && e.Exam_Name.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (e.Subject != null && e.Subject.Subject_Name != null && e.Subject.Subject_Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            return View(exams);
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

            // Lấy danh sách câu hỏi thuộc môn học của bài thi, do giáo viên tạo, và chưa có trong bài thi
            var questions = await _context.Question
                .Where(q => q.CreatorUser_Id == teacherId
                            && q.Subject_ID == exam.Subject_ID // Chỉ lấy câu hỏi thuộc môn học của bài thi
                            && !existingQuestionIds.Contains(q.Question_ID)) // Loại bỏ các câu hỏi đã có trong bài thi
                .Include(q => q.QuestionType) // Bao gồm thông tin loại câu hỏi
                .Include(q => q.Level) // Bao gồm thông tin độ khó
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
                .ThenInclude(eq => eq.QuestionType)
                .Include(eq => eq.Question.Subject) // Bao gồm thông tin môn học
                .Include(eq => eq.Question.Level)
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
        public async Task<IActionResult> GetFilteredQuestions(int? subjectId, int? questionTypeId, int? difficultyLevel)
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);

            // Lấy danh sách câu hỏi theo bộ lọc
            var questionsQuery = _context.Question
                .Where(q => q.CreatorUser_Id == teacherId);

            if (subjectId.HasValue)
            {
                questionsQuery = questionsQuery.Where(q => q.Subject_ID == subjectId.Value);
            }

            if (questionTypeId.HasValue)
            {
                questionsQuery = questionsQuery.Where(q => q.QuestionTypeId == questionTypeId.Value);
            }
            if (difficultyLevel.HasValue)
            {
                questionsQuery = questionsQuery.Where(q => q.Level_ID == difficultyLevel.Value);
            }

            var questions = await questionsQuery
                .Include(q => q.QuestionType)
                .Include(q => q.Subject)
                .Include(q => q.Level)
                .ToListAsync();

            // Trả về JSON mà không bao gồm Options
            return Json(questions);
        }



        [HttpGet]
        public async Task<IActionResult> CreateWithQuestions(int? subjectId, int? questionTypeId , int? difficultyLevel)
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);

            // Lấy danh sách môn học do giáo viên tạo
            ViewData["SubjectId"] = new SelectList(
                await _context.Subjects.Where(s => s.CreatorUser_Id == teacherId).ToListAsync(),
                "Subject_Id",
                "Subject_Name",
                subjectId
            );

            // Lấy danh sách loại câu hỏi
            ViewData["QuestionTypeId"] = new SelectList(
                await _context.QuestionType.ToListAsync(),
                "Id",
                "Name",
                questionTypeId
            );
            // Lấy danh sách loại câu hỏi
            ViewData["DifficultyLevels"] = new SelectList(
                await _context.Levels.ToListAsync(),
                "Id",
                "LevelName",
                difficultyLevel
            );

            // Lấy danh sách câu hỏi theo bộ lọc
            var questionsQuery = _context.Question
                .Where(q => q.CreatorUser_Id == teacherId);

            if (subjectId.HasValue)
            {
                questionsQuery = questionsQuery.Where(q => q.Subject_ID == subjectId.Value);
            }

            if (questionTypeId.HasValue)
            {
                questionsQuery = questionsQuery.Where(q => q.QuestionTypeId == questionTypeId.Value);
            }
            if (difficultyLevel.HasValue)
            {
                questionsQuery = questionsQuery.Where(q => q.Level_ID == difficultyLevel.Value);
            }

            var questions = await questionsQuery
                .Include(q => q.QuestionType)
                .Include(q => q.Subject)
                .Include(q => q.Level)
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
                decimal pointsPerQuestion = questionIds.Count > 0 ? 10m / questionIds.Count : 0;

                // Thêm các câu hỏi vào bài thi
                int currentOrder = 0;
                foreach (var questionId in questionIds)
                {
                    currentOrder++;
                    var examQuestion = new Exam_Question
                    {
                        Exam_ID = exam.Exam_ID,
                        Question_ID = questionId,
                        Question_Order = currentOrder,
                        Points = pointsPerQuestion
                    };
                    _context.ExamQuestions.Add(examQuestion);
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Tạo bài thi và thêm câu hỏi thành công.";
                return RedirectToAction(nameof(Index));
            }

            // Nếu có lỗi, hiển thị lại form
            var teacherId = int.Parse(HttpContext.Session.GetString("UserId"));
            ViewData["SubjectId"] = new SelectList(
                await _context.Subjects.Where(s => s.CreatorUser_Id == teacherId).ToListAsync(),"Subject_Id","Subject_Name");
            ViewData["QuestionTypeId"] = new SelectList(
                await _context.QuestionType.ToListAsync(),"Id","Name");
            ViewData["DifficultyLevels"] = new SelectList(
               await _context.Levels.ToListAsync(), "Id", "LevelName");
            ViewData["Questions"] = await _context.Question
                .Where(q => q.CreatorUser_Id == teacherId)
                .Include(q => q.QuestionType)
                .Include(q => q.Subject)
                .Include(q => q.Level)
                .ToListAsync();

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
        [HttpGet]
        public async Task<IActionResult> ManageExam(int examId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);

            // Lấy thông tin bài thi
            var exam = await _context.Exams
                .Include(e => e.Subject)
                .FirstOrDefaultAsync(e => e.Exam_ID == examId && e.CreatorUser_Id == teacherId);

            if (exam == null)
            {
                return Unauthorized();
            }

            // Lấy danh sách câu hỏi đã thêm vào bài thi
            var examQuestions = await _context.ExamQuestions
                .Where(eq => eq.Exam_ID == examId)
                .Include(eq => eq.Question)
                .ThenInclude(q => q.QuestionType)
                .Include(eq => eq.Question.Level)
                .OrderBy(eq => eq.Question_Order)
                .ToListAsync();

            // Lấy danh sách câu hỏi có thể thêm vào bài thi
            var existingQuestionIds = examQuestions.Select(eq => eq.Question_ID).ToList();
            var availableQuestions = await _context.Question
                .Where(q => q.CreatorUser_Id == teacherId
                            && q.Subject_ID == exam.Subject_ID
                            && !existingQuestionIds.Contains(q.Question_ID))
                .Include(q => q.QuestionType)
                .Include(q => q.Level)
                .ToListAsync();

            var viewModel = new ManageExamViewModel
            {
                Exam = exam ?? new Exam(),
                ExamQuestions = examQuestions ?? new List<Exam_Question>(),
                AvailableQuestions = availableQuestions ?? new List<Question>()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ManageExam(ManageExamViewModel model, string actionType, int? questionId, List<int>? questionIds)
        {
            if (model.Exam == null || model.Exam.Exam_ID == 0)
            {
                TempData["ErrorMessage"] = "Dữ liệu bài thi không hợp lệ.";
                return RedirectToAction("Index");
            }

            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);

            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Exam_ID == model.Exam.Exam_ID && e.CreatorUser_Id == teacherId);
            if (exam == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài thi.";
                return RedirectToAction("Index");
            }

            switch (actionType)
            {
                case "Edit":
                    exam.Exam_Name = model.Exam.Exam_Name;
                    exam.TotalQuestions = model.Exam.TotalQuestions;
                    exam.Duration = model.Exam.Duration;
                    exam.PassScore = model.Exam.PassScore;
                    exam.IsActive = model.Exam.IsActive;
                    exam.Exam_Date = model.Exam.Exam_Date.Date;

                    var startTime = model.Exam.StartTime.TimeOfDay;
                    var endTime = model.Exam.EndTime.TimeOfDay;
                    exam.StartTime = model.Exam.Exam_Date.Date.Add(startTime);
                    exam.EndTime = model.Exam.Exam_Date.Date.Add(endTime);

                    if (exam.StartTime >= exam.EndTime)
                    {
                        TempData["ErrorMessage"] = "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc!";
                        return RedirectToAction("ManageExam", new { examId = model.Exam.Exam_ID });
                    }

                    _context.Exams.Update(exam);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật bài thi thành công.";
                    break;




                case "AddQuestion":
                    if (questionIds != null && questionIds.Any())
                    {
                        var currentCount = await _context.ExamQuestions
                            .CountAsync(eq => eq.Exam_ID == model.Exam.Exam_ID);

                        int ttotalQuestions = model.Exam.TotalQuestions;
                        int canAdd = ttotalQuestions - currentCount;

                        if (questionIds.Count > canAdd)
                        {
                            TempData["ErrorMessage"] = "Không thể thêm câu hỏi vượt quá số lượng câu hỏi trong bài thi!";
                            return RedirectToAction("ManageExam", new { examId = model.Exam.Exam_ID });
                        }

                        var currentOrder = await _context.ExamQuestions
                            .Where(eq => eq.Exam_ID == model.Exam.Exam_ID)
                            .MaxAsync(eq => (int?)eq.Question_Order) ?? 0;

                        foreach (var qid in questionIds)
                        {
                            if (!_context.ExamQuestions.Any(eq => eq.Exam_ID == model.Exam.Exam_ID && eq.Question_ID == qid))
                            {
                                currentOrder++;
                                var examQuestion = new Exam_Question
                                {
                                    Exam_ID = model.Exam.Exam_ID,
                                    Question_ID = qid,
                                    Question_Order = currentOrder,
                                    Points = 0
                                };
                                _context.ExamQuestions.Add(examQuestion);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
                    break;


                case "RemoveQuestion":
                    if (questionId.HasValue)
                    {
                        var examQuestion = await _context.ExamQuestions
                            .FirstOrDefaultAsync(eq => eq.Exam_ID == model.Exam.Exam_ID && eq.Question_ID == questionId.Value);

                        if (examQuestion != null)
                        {
                            _context.ExamQuestions.Remove(examQuestion);
                            await _context.SaveChangesAsync();
                        }
                    }
                    break;
            }

            // Tính lại điểm cho tất cả các câu hỏi trong bài thi
            var allQuestions = await _context.ExamQuestions
                .Where(eq => eq.Exam_ID == model.Exam.Exam_ID)
                .OrderBy(eq => eq.Question_Order)
                .ToListAsync();

            int totalQuestions = allQuestions.Count;
            decimal pointsPerQuestion = totalQuestions > 0 ? 10m / totalQuestions : 0;

            int newOrder = 1;
            foreach (var question in allQuestions)
            {
                question.Question_Order = newOrder++;
                question.Points = pointsPerQuestion;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật bài thi thành công.";
            return RedirectToAction("ManageExam", new { examId = model.Exam.Exam_ID });
        }





    }
}
