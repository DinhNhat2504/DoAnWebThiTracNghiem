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
                .Where(e => e.CreatorUser_Id == teacherId)
                .OrderByDescending(e => e.CreateAt)// Lọc theo giáo viên
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

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            var examQuestions = _context.ExamQuestions.Where(eq => eq.Exam_ID == id);
            var stafterfix = await _context.ExamResult.Where(c => c.Exam_ID == id).ToListAsync();
            var relatedAnswers = await _context.Answers.Where(a => stafterfix.Select(e => e.Result_ID).Contains(a.Result_ID1)).ToListAsync();
            _context.Answers.RemoveRange(relatedAnswers);
            await _context.SaveChangesAsync();
            _context.ExamResult.RemoveRange(stafterfix);
            _context.ExamQuestions.RemoveRange(examQuestions);
            await _examRepository.DeleteAsync(id);
            TempData["SuccessMessage"] = "Xóa bài thi thành công !";
            return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> FilterExamQuestions(int examId, int? questionTypeId, int? difficultyLevel)
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);

            var exam = await _context.Exams
                .FirstOrDefaultAsync(e => e.Exam_ID == examId && e.CreatorUser_Id == teacherId);

            if (exam == null)
                return NotFound();

            // Câu hỏi đã thêm vào bài thi
            IQueryable<Exam_Question> examQuestionsQuery = _context.ExamQuestions
            .Where(eq => eq.Exam_ID == examId)
            .Include(eq => eq.Question)
                .ThenInclude(q => q.QuestionType)
            .Include(eq => eq.Question)
                .ThenInclude(q => q.Level);

            if (questionTypeId.HasValue)
                examQuestionsQuery = examQuestionsQuery.Where(eq => eq.Question.QuestionTypeId == questionTypeId.Value);
            if (difficultyLevel.HasValue)
                examQuestionsQuery = examQuestionsQuery.Where(eq => eq.Question.Level_ID == difficultyLevel.Value);

            var examQuestions = await examQuestionsQuery
                .OrderBy(eq => eq.Question_Order)
                .Select(eq => new
                {
                    eq.Question_ID,
                    eq.Points,
                    eq.Question.Question_Content,
                    QuestionType = eq.Question.QuestionType.Name,
                    Level = eq.Question.Level.LevelName
                }).ToListAsync();

            // Câu hỏi chưa thêm vào bài thi
            var existingQuestionIds = await _context.ExamQuestions
                .Where(eq => eq.Exam_ID == examId)
                .Select(eq => eq.Question_ID)
                .ToListAsync();

            var availableQuestionsQuery = _context.Question
                .Where(q => q.CreatorUser_Id == teacherId
                            && q.Subject_ID == exam.Subject_ID
                            && !existingQuestionIds.Contains(q.Question_ID));

            if (questionTypeId.HasValue)
                availableQuestionsQuery = availableQuestionsQuery.Where(q => q.QuestionTypeId == questionTypeId.Value);
            if (difficultyLevel.HasValue)
                availableQuestionsQuery = availableQuestionsQuery.Where(q => q.Level_ID == difficultyLevel.Value);

            var availableQuestions = await availableQuestionsQuery
                .Include(q => q.QuestionType)
                .Include(q => q.Level)
                .Select(q => new
                {
                    q.Question_ID,
                    q.Question_Content,
                    QuestionType = q.QuestionType.Name,
                    Level = q.Level.LevelName
                }).ToListAsync();

            return Json(new { examQuestions, availableQuestions });
        }

        [HttpGet]
        public async Task<IActionResult> CreateWithQuestions(int? subjectId, int? questionTypeId, int? difficultyLevel)
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
            var userId = HttpContext.Session.GetString("UserId");
            var teacherId = int.Parse(HttpContext.Session.GetString("UserId"));
            if (ModelState.IsValid)
            {

                // Kiểm tra trùng tên bài thi trong cùng môn học (không phân biệt hoa thường)
                var isDuplicate = await _context.Exams.AnyAsync(e =>
                    e.Subject_ID == exam.Subject_ID &&
                    e.Exam_Name.ToLower() == exam.Exam_Name.ToLower());

                if (isDuplicate)
                {
                    TempData["ErrorMessage"] = "Tên bài thi của môn học này đã tồn tại. Vui lòng chọn tên khác.";

                    ViewData["SubjectId"] = new SelectList(
                        await _context.Subjects.Where(s => s.CreatorUser_Id == teacherId).ToListAsync(), "Subject_Id", "Subject_Name");
                    ViewData["QuestionTypeId"] = new SelectList(
                        await _context.QuestionType.ToListAsync(), "Id", "Name");
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

                exam.CreatorUser_Id = int.Parse(userId);
                exam.CreateAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

                // Ghép ngày và giờ
                var userStart = DateTime.SpecifyKind(exam.Exam_Date.Date + exam.StartTime.TimeOfDay, DateTimeKind.Unspecified);
                var userEnd = DateTime.SpecifyKind(exam.Exam_Date.Date + exam.EndTime.TimeOfDay, DateTimeKind.Unspecified);

                exam.StartTime = TimeZoneInfo.ConvertTimeToUtc(userStart, timeZone);
                exam.EndTime = TimeZoneInfo.ConvertTimeToUtc(userEnd, timeZone);

                // Tự động set tổng số câu hỏi
                exam.TotalQuestions = questionIds.Count;

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

            ViewData["SubjectId"] = new SelectList(
                await _context.Subjects.Where(s => s.CreatorUser_Id == teacherId).ToListAsync(), "Subject_Id", "Subject_Name");
            ViewData["QuestionTypeId"] = new SelectList(
                await _context.QuestionType.ToListAsync(), "Id", "Name");
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
        public async Task<IActionResult> ManageExam(int examId, int? questionTypeId = null, int? difficultyLevel = null)
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);

            // Lấy thông tin bài thi
            var exam = await _context.Exams
                .Include(e => e.Subject)
                .FirstOrDefaultAsync(e => e.Exam_ID == examId && e.CreatorUser_Id == teacherId);

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            exam.StartTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(exam.StartTime, DateTimeKind.Utc), timeZone);
            exam.EndTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(exam.EndTime, DateTimeKind.Utc), timeZone);

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
            var availableQuestionsQuery = _context.Question
                .Where(q => q.CreatorUser_Id == teacherId
                            && q.Subject_ID == exam.Subject_ID
                            && !existingQuestionIds.Contains(q.Question_ID));

            // Áp dụng bộ lọc nếu có
            if (questionTypeId.HasValue)
            {
                availableQuestionsQuery = availableQuestionsQuery.Where(q => q.QuestionTypeId == questionTypeId.Value);
            }
            if (difficultyLevel.HasValue)
            {
                availableQuestionsQuery = availableQuestionsQuery.Where(q => q.Level_ID == difficultyLevel.Value);
            }

            var availableQuestions = await availableQuestionsQuery
                .Include(q => q.QuestionType)
                .Include(q => q.Level)
                .ToListAsync();

            // Thêm SelectLists cho bộ lọc
            ViewData["QuestionTypeId"] = new SelectList(await _context.QuestionType.ToListAsync(), "Id", "Name");
            ViewData["DifficultyLevels"] = new SelectList(await _context.Levels.ToListAsync(), "Id", "LevelName");

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
            var currentCount = await _context.ExamQuestions
                            .CountAsync(eq => eq.Exam_ID == model.Exam.Exam_ID);
            var stafterfix = await _context.ExamResult.Where(c => c.Exam_ID == model.Exam.Exam_ID).ToListAsync();
            var relatedAnswers = await _context.Answers.Where(a => stafterfix.Select(e => e.Result_ID).Contains(a.Result_ID1)).ToListAsync();


            switch (actionType)
            {
                case "Edit":
                    var isDuplicate = await _context.Exams.AnyAsync(e =>
                    e.Subject_ID == model.Exam.Subject_ID &&
                    e.Exam_ID != model.Exam.Exam_ID &&
                    e.Exam_Name.ToLower() == model.Exam.Exam_Name.ToLower());

                    if (isDuplicate)
                    {
                        TempData["MnErrorMessage"] = "Tên bài thi của môn học này đã tồn tại. Vui lòng chọn tên khác.";
                        return RedirectToAction("ManageExam", new { examId = model.Exam.Exam_ID });
                    }
                    exam.Exam_Name = model.Exam.Exam_Name;
                    exam.TotalQuestions = model.Exam.TotalQuestions;
                    exam.Duration = model.Exam.Duration;
                    exam.PassScore = model.Exam.PassScore;
                    exam.CreateAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                    exam.Exam_Date = model.Exam.Exam_Date.Date;
                    var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

                    // Lấy giờ phút từ model.Exam (giá trị người dùng nhập trên form)
                    var userStart = DateTime.SpecifyKind(exam.Exam_Date.Date + model.Exam.StartTime.TimeOfDay, DateTimeKind.Unspecified);
                    var userEnd = DateTime.SpecifyKind(exam.Exam_Date.Date + model.Exam.EndTime.TimeOfDay, DateTimeKind.Unspecified);

                    // Chuyển sang UTC trước khi lưu
                    exam.StartTime = TimeZoneInfo.ConvertTimeToUtc(userStart, timeZone);
                    exam.EndTime = TimeZoneInfo.ConvertTimeToUtc(userEnd, timeZone);

                    if (exam.StartTime >= exam.EndTime)
                    {
                        TempData["MnErrorMessage"] = "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc!";
                        return RedirectToAction("ManageExam", new { examId = model.Exam.Exam_ID });
                    }

                    //if (exam.TotalQuestions < currentCount)
                    //{
                    //    TempData["MnErrorMessage"] = "Số lượng câu hỏi không thể nhỏ hơn số câu hỏi hiện tại trong bài thi!";
                    //    return RedirectToAction("ManageExam", new { examId = model.Exam.Exam_ID });
                    //}
                    _context.Exams.Update(exam);
                    await _context.SaveChangesAsync();
                    TempData["MnSuccessMessage"] = "Cập nhật bài thi thành công.";
                    break;

                case "AddQuestion":
                    if (questionIds == null || !questionIds.Any())
                    {
                        TempData["MnErrorMessage"] = "Vui lòng chọn ít nhất một câu hỏi để thêm vào bài thi.";
                        return RedirectToAction("ManageExam", new { examId = model.Exam.Exam_ID });
                    }

                    int maxQuestions = exam.TotalQuestions;
                    int canAdd = maxQuestions - currentCount;



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

                    _context.Answers.RemoveRange(relatedAnswers);
                    await _context.SaveChangesAsync();

                    _context.ExamResult.RemoveRange(stafterfix);
                    await _context.SaveChangesAsync();

                    // Cập nhật lại tổng số câu hỏi
                    exam.TotalQuestions = await _context.ExamQuestions.CountAsync(eq => eq.Exam_ID == exam.Exam_ID);
                    _context.Exams.Update(exam);
                    await _context.SaveChangesAsync();
                    break;

                case "RemoveQuestion":
                    if (questionId.HasValue)
                    {
                        var examQuestion = await _context.ExamQuestions
                            .FirstOrDefaultAsync(eq => eq.Exam_ID == model.Exam.Exam_ID && eq.Question_ID == questionId.Value);

                        if (examQuestion != null)
                        {
                            _context.Answers.RemoveRange(relatedAnswers);
                            await _context.SaveChangesAsync();
                            _context.ExamQuestions.Remove(examQuestion);
                            _context.ExamResult.RemoveRange(stafterfix);
                            await _context.SaveChangesAsync();
                        }
                    }

                    // Cập nhật lại tổng số câu hỏi
                    exam.TotalQuestions = await _context.ExamQuestions.CountAsync(eq => eq.Exam_ID == exam.Exam_ID);
                    _context.Exams.Update(exam);
                    await _context.SaveChangesAsync();
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

            TempData["MnSuccessMessage"] = "Cập nhật bài thi thành công.";
            return RedirectToAction("ManageExam", new { examId = model.Exam.Exam_ID });
        }
 

    }
}
