using DoAnWebThiTracNghiem.Areas.Student.Models;
using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnWebThiTracNghiem.Areas.Student.Controllers
{
    [Area("Student")]
    public class StudentTakeExamController : Controller
    {
        private readonly AppDBContext _context;
        public StudentTakeExamController(AppDBContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> ExamList()
        {
            var userId = HttpContext.Session.GetString("UserId");
            int studentId = int.Parse(userId);

            // Lấy danh sách lớp học mà sinh viên tham gia
            var classIds = await _context.ClassStudents
                .Where(cs => cs.User_ID == studentId)
                .Select(cs => cs.Class_ID)
                .ToListAsync();

            // Lấy danh sách bài thi được giao cho các lớp học đó
            var exams = await _context.ClassExams
                .Where(ce => classIds.Contains(ce.ClassTNClass_Id))
                .Select(ce => ce.Exam)
                .Where(e => e.IsActive) // Chỉ lấy bài thi đang hoạt động
                .ToListAsync();

            // Lấy danh sách bài thi đã làm
            var takenExams = await _context.ExamResult
                .Where(er => er.User_ID == studentId)
                .Select(er => er.Exam_ID)
                .ToListAsync();

            ViewData["TakenExams"] = takenExams;

            return View(exams);
        }

        [HttpGet]
        public async Task<IActionResult> TakeExam(int examId, int page = 1)
        {
            // Kiểm tra UserId trong session
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để làm bài thi.";
                return RedirectToAction("Login", "Users");
            }
            // Kiểm tra định dạng UserId
            var studentId = int.Parse(userId);
            try
            {
                // Kiểm tra quyền truy cập
                var classExams = await _context.ClassExams
                    .Where(ce => ce.Exam_ID == examId)
                    .ToListAsync();


                var classStudents = await _context.ClassStudents
                    .Where(cs => cs.User_ID == studentId)
                    .ToListAsync();


                var isAssigned = await _context.ClassExams
                    .AnyAsync(ce => ce.Exam_ID == examId &&
                                    _context.ClassStudents.Any(cs => cs.User_ID == studentId && cs.Class_ID == ce.ClassTNClass_Id));

                if (!isAssigned)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền truy cập bài thi này.";
                    return RedirectToAction("Index", "StudentExam");
                }

                // Lấy thông tin bài thi
                var exam = await _context.Exams
                    .Include(e => e.Exam_Questions)
                    .ThenInclude(eq => eq.Question)
                    .FirstOrDefaultAsync(e => e.Exam_ID == examId);

                if (exam == null)
                {

                    TempData["ErrorMessage"] = "Không tìm thấy bài thi.";
                    return RedirectToAction("Index", "StudentExam");
                }

                // Kiểm tra thời gian hợp lệ
                var currentTime = DateTime.Now;
                if (currentTime > exam.EndTime || currentTime.Date != exam.Exam_Date.Date)
                {

                    TempData["ErrorMessage"] = "Bài thi đã hết hạn hoặc không hợp lệ.";
                    return RedirectToAction("Index", "StudentExam");
                }

                // Lưu và lấy thời gian bắt đầu
                string startTimeKey = $"StartTime_{examId}_{studentId}";
                DateTime startTime;
                string startTimeString = HttpContext.Session.GetString(startTimeKey);
                if (string.IsNullOrEmpty(startTimeString))
                {
                    startTime = DateTime.Now;
                    HttpContext.Session.SetString(startTimeKey, startTime.ToString("o"));
                }
                else
                {
                    startTime = DateTime.Parse(startTimeString);
                }

                // Tính thời gian còn lại
                var elapsedTime = (DateTime.Now - startTime).TotalSeconds;
                var totalDurationInSeconds = exam.Duration * 60;
                var remainingTime = Math.Max(0, totalDurationInSeconds - elapsedTime);

                // Phân trang
                var allQuestions = exam.Exam_Questions?.OrderBy(q => q.Question_Order).ToList() ?? new List<Exam_Question>();
                int pageSize = 10;
                int totalQuestions = allQuestions.Count;
                int totalPages = (int)Math.Ceiling((double)totalQuestions / pageSize);
                page = Math.Max(1, Math.Min(page, totalPages));

                var questionsToDisplay = allQuestions
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Lấy trạng thái đáp án từ session
                var sessionData = HttpContext.Session.GetString($"ExamAnswers_{examId}");
                var answers = sessionData?.Split(';')
                    .Select(a => a.Split('='))
                    .ToDictionary(x => int.Parse(x[0]), x => x[1]) ?? new Dictionary<int, string>();
                // Sau khi lấy allQuestions
                var questionIds = allQuestions.Select(q => q.Question_ID).ToList();
                HttpContext.Session.SetString($"ExamQuestionIds_{examId}", string.Join(",", questionIds));
                // lưu thông tin thời gian vào session 
                HttpContext.Session.SetString($"ExamDate_{examId}", exam.Exam_Date.ToString("yyyy-MM-dd"));
                HttpContext.Session.SetString($"ExamStartTime_{examId}", exam.StartTime.ToString("o"));
                HttpContext.Session.SetString($"ExamEndTime_{examId}", exam.EndTime.ToString("o"));

                // Truyền dữ liệu vào ViewData
                ViewData["Questions"] = questionsToDisplay;
                ViewData["AllQuestions"] = allQuestions;
                ViewData["RemainingTime"] = remainingTime;
                ViewData["CurrentPage"] = page;
                ViewData["TotalPages"] = totalPages;
                ViewData["ExamId"] = examId;
                ViewData["Answers"] = answers;

                return View(exam);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TakeExam] Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải bài thi. Vui lòng thử lại.";
                return RedirectToAction("Index", "StudentExam");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitExam(int examId, IFormCollection form)
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Người dùng chưa đăng nhập hoặc session đã hết hạn.";
                return RedirectToAction("Login", "Users");
            }

            var studentId = int.Parse(userId);
            // Kiểm tra bài thi còn tồn tại không
            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Exam_ID == examId);
            if (exam == null)
            {
                TempData["ErrorMessage"] = "Bài thi đã bị xóa hoặc không còn tồn tại.";
                // Xóa session liên quan nếu cần
                HttpContext.Session.Remove($"ExamAnswers_{examId}");
                HttpContext.Session.Remove($"StartTime_{examId}_{studentId}");
                HttpContext.Session.Remove($"ExamQuestionIds_{examId}");
                return RedirectToAction("Index", "StudentExam");
            }

            // Kiểm tra ngày thi/thời gian thi thay đổi
            var sessionExamDate = HttpContext.Session.GetString($"ExamDate_{examId}");
            var sessionStartTime = HttpContext.Session.GetString($"ExamStartTime_{examId}");
            var sessionEndTime = HttpContext.Session.GetString($"ExamEndTime_{examId}");

            if (sessionExamDate == null || sessionStartTime == null || sessionEndTime == null
                || exam.Exam_Date.ToString("yyyy-MM-dd") != sessionExamDate
                || exam.StartTime.ToString("o") != sessionStartTime
                || exam.EndTime.ToString("o") != sessionEndTime)
            {
                TempData["ErrorMessage"] = "Thông tin bài thi đã bị thay đổi trong quá trình làm bài. Vui lòng liên hệ giáo viên hoặc thử lại.";
                HttpContext.Session.Remove($"ExamAnswers_{examId}");
                HttpContext.Session.Remove($"StartTime_{examId}_{studentId}");
                HttpContext.Session.Remove($"ExamQuestionIds_{examId}");
                HttpContext.Session.Remove($"ExamDate_{examId}");
                HttpContext.Session.Remove($"ExamStartTime_{examId}");
                HttpContext.Session.Remove($"ExamEndTime_{examId}");
                return RedirectToAction("Index", "StudentExam");
            }


            try
            {
                var isAssigned = await _context.ClassExams
                    .AnyAsync(ce => ce.Exam_ID == examId &&
                                    _context.ClassStudents.Any(cs => cs.User_ID == studentId && cs.Class_ID == ce.ClassTNClass_Id));

                if (!isAssigned)
                {
                    TempData["ErrorMessage"] = "Bài thi không được gán cho sinh viên này.";
                    return RedirectToAction("Index", "StudentExam");
                }

                // Lấy danh sách câu hỏi
                var questions = await _context.ExamQuestions
                    .Where(eq => eq.Exam_ID == examId)
                    .Include(eq => eq.Question)
                    .ToListAsync();
                // Lấy danh sách ID câu hỏi đã lưu trong session
                var sessionQuestionIdsStr = HttpContext.Session.GetString($"ExamQuestionIds_{examId}");
                var sessionQuestionIds = sessionQuestionIdsStr?.Split(',').Select(int.Parse).ToList() ?? new List<int>();

                var dbQuestionIds = questions.Select(q => q.Question_ID).OrderBy(x => x).ToList();
                var sessionQuestionIdsOrdered = sessionQuestionIds.OrderBy(x => x).ToList();

                // So sánh số lượng và danh sách ID 
                if (sessionQuestionIds.Count == 0 || sessionQuestionIds.Count != dbQuestionIds.Count || !sessionQuestionIdsOrdered.SequenceEqual(dbQuestionIds))
                {
                    TempData["ErrorMessage"] = "Bài thi đã bị thay đổi trong quá trình làm bài. Vui lòng liên hệ giáo viên hoặc thử lại.";
                    // Xóa session để tránh lỗi lần sau
                    HttpContext.Session.Remove($"ExamAnswers_{examId}");
                    HttpContext.Session.Remove($"StartTime_{examId}_{studentId}");
                    HttpContext.Session.Remove($"ExamQuestionIds_{examId}");
                    return RedirectToAction("Index", "StudentExam");
                }


                // Lấy trạng thái đáp án từ session (ưu tiên session)
                var answers = HttpContext.Session.GetString($"ExamAnswers_{examId}")?.Split(';')
                    .Select(a => a.Split('='))
                    .ToDictionary(x => int.Parse(x[0]), x => x[1]) ?? new Dictionary<int, string>();

                // Cập nhật từ form (nếu có)
                foreach (var key in form.Keys)
                {
                    if (key.StartsWith("answer-"))
                    {
                        var questionId = int.Parse(key.Replace("answer-", ""));
                        answers[questionId] = form[key];
                    }
                }

                decimal totalScore = 0;
                decimal maxScore = 0;
                var studentAnswers = new List<Student_Answers>();

                // Tính điểm và lưu đáp án
                foreach (var question in questions)
                {
                    maxScore += question.Points;

                    var answerKey = question.Question_ID;
                    var studentAnswer = answers.ContainsKey(answerKey) ? answers[answerKey] : null;

                    if (!string.IsNullOrEmpty(studentAnswer))
                    {
                        var normalizedStudentAnswer = studentAnswer.Trim().ToLower();
                        var normalizedCorrectOption = question.Question.Correct_Option?.Trim().ToLower() ?? string.Empty;
                        normalizedStudentAnswer = normalizedStudentAnswer.Replace("\r", "").Replace("\n", "");
                        normalizedCorrectOption = normalizedCorrectOption.Replace("\r", "").Replace("\n", "");

                        bool isCorrect = normalizedStudentAnswer == normalizedCorrectOption;

                        if (isCorrect)
                        {
                            totalScore += question.Points;
                        }

                        studentAnswers.Add(new Student_Answers
                        {
                            Question_ID = question.Question_ID,
                            Selected_Option = studentAnswer,
                            Is_Correct = isCorrect
                        });
                    }
                    else
                    {
                        Console.WriteLine($"[SubmitExam] No answer provided for Question ID: {question.Question_ID}");
                    }
                }


                // Lấy thời gian bắt đầu từ session
                string startTimeKey = $"StartTime_{examId}_{studentId}";
                string startTimeString = HttpContext.Session.GetString(startTimeKey);
                DateTime startTime = startTimeString != null ? DateTime.Parse(startTimeString) : DateTime.Now;

                // Lưu Exam_Result
                var examResult = new Exam_Result
                {
                    Exam_ID = examId,
                    User_ID = studentId,
                    CorrectAnswers = studentAnswers.Count(sa => sa.Is_Correct),
                    WrongAnswers = studentAnswers.Count(sa => !sa.Is_Correct),
                    Score = maxScore > 0 ? totalScore / maxScore * 10 : 0,
                    Start_Time = startTime,
                    End_Time = DateTime.Now
                };

                _context.ExamResult.Add(examResult);
                await _context.SaveChangesAsync();

                // Gán Result_ID1 cho studentAnswers
                if (studentAnswers.Any())
                {
                    foreach (var answer in studentAnswers)
                    {
                        answer.Result_ID1 = examResult.Result_ID;
                    }
                    _context.Answers.AddRange(studentAnswers);
                    await _context.SaveChangesAsync();
                }
                // Xóa session
                HttpContext.Session.Remove($"ExamAnswers_{examId}");
                HttpContext.Session.Remove(startTimeKey);
                TempData["SuccessMessage"] = "Nộp bài thành công!";
                return RedirectToAction("Index", "StudentExam");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TakeExam] Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi nộph bài thi. Vui lòng thử lại.";
                return RedirectToAction("Index", "StudentExam");
            }


        }



        public async Task<IActionResult> ViewResult(int examId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            int studentId = int.Parse(userId);

            // Lấy kết quả bài thi
            var examResult = await _context.ExamResult
                .Include(er => er.Exam)
                .FirstOrDefaultAsync(er => er.Exam_ID == examId && er.User_ID == studentId);

            if (examResult == null)
            {
                return NotFound();
            }

            // Lấy danh sách câu trả lời của sinh viên, bao gồm QuestionType
            var studentAnswers = await _context.Answers
                .Where(sa => sa.Result_ID1 == examResult.Result_ID)
                .Include(sa => sa.Question)
                .ThenInclude(q => q.QuestionType) // Bao gồm QuestionType
                .Include(sa => sa.Question.Exam_Questions) // Bao gồm Exam_Questions
                .ToListAsync();

            // Chuẩn bị ViewModel
            var viewModel = new ViewResultViewModel
            {
                Exam = examResult.Exam,
                Score = examResult.Score,
                CorrectAnswers = examResult.CorrectAnswers,
                WrongAnswers = examResult.WrongAnswers,
                StudentAnswers = studentAnswers
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult SaveAnswer([FromBody] AnswerModel request)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("[SaveAnswer] Error: UserId is null or empty");
                return Json(new { success = false, message = "Người dùng chưa đăng nhập." });
            }

            int studentId = int.Parse(userId);
            string sessionKey = $"ExamAnswers_{request.ExamId}";
            var answers = HttpContext.Session.GetString(sessionKey)?.Split(';')
                .Select(a => a.Split('='))
                .ToDictionary(x => int.Parse(x[0]), x => x[1]) ?? new Dictionary<int, string>();

            // Lưu đáp án, xử lý trường hợp answer là null
            answers[request.QuestionId] = request.Answer ?? "";
            string sessionData = string.Join(";", answers.Select(kv => $"{kv.Key}={kv.Value}"));
            HttpContext.Session.SetString(sessionKey, sessionData);

            Console.WriteLine($"[SaveAnswer] Saved answer for exam {request.ExamId}, question {request.QuestionId}, answer: '{request.Answer}', session data: {sessionData}");
            return Json(new { success = true });
        }




    }
}
