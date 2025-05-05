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
        public async Task<IActionResult> TakeExam(int examId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            Console.WriteLine($"UserId in TakeExam: {userId}, examId: {examId}");

            int studentId = int.Parse(userId);

            var classExams = await _context.ClassExams
                .Where(ce => ce.Exam_ID == examId)
                .ToListAsync();
            Console.WriteLine($"TakeExam - ClassExams count for Exam_ID {examId}: {classExams.Count}");
            foreach (var ce in classExams)
            {
                Console.WriteLine($"TakeExam - ClassExams: Exam_ID = {ce.Exam_ID}, ClassTNClass_Id = {ce.ClassTNClass_Id}");
            }

            var classStudents = await _context.ClassStudents
                .Where(cs => cs.User_ID == studentId)
                .ToListAsync();
            Console.WriteLine($"TakeExam - ClassStudents count for User_ID {studentId}: {classStudents.Count}");
            foreach (var cs in classStudents)
            {
                Console.WriteLine($"TakeExam - ClassStudents: User_ID = {cs.User_ID}, Class_ID = {cs.Class_ID}");
            }

            var isAssigned = await _context.ClassExams
                .AnyAsync(ce => ce.Exam_ID == examId &&
                                _context.ClassStudents.Any(cs => cs.User_ID == studentId && cs.Class_ID == ce.ClassTNClass_Id));

            Console.WriteLine($"TakeExam - isAssigned: {isAssigned}");

            if (!isAssigned)
            {
                return Unauthorized();
            }

            var exam = await _context.Exams
                .Include(e => e.Exam_Questions)
                .ThenInclude(eq => eq.Question)
                .FirstOrDefaultAsync(e => e.Exam_ID == examId);

            if (exam == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài thi.";
                return RedirectToAction("ExamList");
            }

            var currentTime = DateTime.Now;
            if (currentTime > exam.EndTime || currentTime.Date != exam.Exam_Date.Date)
            {
                Console.WriteLine(currentTime + "Bài thi đã hết hạn hoặc không hợp lệ");
                Console.WriteLine(exam.EndTime + "Bài thi đã hết hạn hoặc không hợp lệ");
                TempData["ErrorMessage"] = "Bài thi đã hết hạn hoặc không hợp lệ.";
                return RedirectToAction("ExamList");
            }

            // Kiểm tra và gán giá trị Duration
            int durationInMinutes = exam.Duration; // Nếu Duration là nullable
            if (durationInMinutes <= 0)
            {
                durationInMinutes = 40; // Giá trị mặc định (từ bảng dữ liệu của bạn)
                Console.WriteLine("Duration không hợp lệ, đặt mặc định là 40 phút");
            }
            var remainingTime = durationInMinutes * 60; // Chuyển sang giây
            

            ViewData["Questions"] = exam.Exam_Questions.OrderBy(q => q.Question_Order).ToList();
            ViewData["RemainingTime"] = remainingTime;
            return View(exam);
        }


        [HttpPost]
        public async Task<IActionResult> SubmitExam(int examId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            Console.WriteLine($"UserId in SubmitExam: {userId}");

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Người dùng chưa đăng nhập hoặc session đã hết hạn.";
                return RedirectToAction("Login", "Users");
            }

            int studentId;
            if (!int.TryParse(userId, out studentId))
            {
                TempData["ErrorMessage"] = "ID sinh viên không hợp lệ.";
                return RedirectToAction("ExamList");
            }

            Console.WriteLine($"examId: {examId}, studentId: {studentId}");

            var isAssigned = await _context.ClassExams
                .AnyAsync(ce => ce.Exam_ID == examId &&
                                _context.ClassStudents.Any(cs => cs.User_ID == studentId && cs.Class_ID == ce.ClassTNClass_Id));

            Console.WriteLine($"isAssigned: {isAssigned}");

            if (!isAssigned)
            {
                TempData["ErrorMessage"] = "Bài thi không được gán cho sinh viên này.";
                return RedirectToAction("ExamList");
            }

            // Lấy danh sách câu hỏi
            var questions = await _context.ExamQuestions
                .Where(eq => eq.Exam_ID == examId)
                .Include(eq => eq.Question)
                .ToListAsync();

            Console.WriteLine($"Total questions: {questions.Count}");

            // In toàn bộ dữ liệu từ Request.Form để debug
            Console.WriteLine("Form Data:");
            foreach (var key in Request.Form.Keys)
            {
                Console.WriteLine($"Key: {key}, Value: {Request.Form[key]}");
            }

            decimal totalScore = 0;
            decimal maxScore = 0;
            var studentAnswers = new List<Student_Answers>();

            // Lấy dữ liệu từ Request.Form với kiểm tra key
            foreach (var question in questions)
            {
                maxScore += question.Points;

                var answerKey = $"answer-{question.Question_ID}";
                var studentAnswerValues = Request.Form[answerKey]; // Lấy StringValues
                var studentAnswer = studentAnswerValues.ToString(); // Chuyển sang string
                Console.WriteLine($"Question ID: {question.Question_ID}, Answer Key: {answerKey}, Student Answer: '{studentAnswer}', Correct Option: '{question.Question.Correct_Option}'");

                if (!string.IsNullOrEmpty(studentAnswer))
                {
                    // Chuẩn hóa dữ liệu trước khi so sánh
                    var normalizedStudentAnswer = studentAnswer.Trim().ToLower();
                    var normalizedCorrectOption = question.Question.Correct_Option?.Trim().ToLower() ?? string.Empty;

                    // Loại bỏ ký tự xuống dòng hoặc ký tự đặc biệt
                    normalizedStudentAnswer = normalizedStudentAnswer.Replace("\r", "").Replace("\n", "");
                    normalizedCorrectOption = normalizedCorrectOption.Replace("\r", "").Replace("\n", "");

                    bool isCorrect = normalizedStudentAnswer == normalizedCorrectOption;
                    Console.WriteLine($"Normalized Student Answer: '{normalizedStudentAnswer}', Normalized Correct Option: '{normalizedCorrectOption}', Is Correct: {isCorrect}");

                    if (isCorrect)
                    {
                        totalScore += question.Points;
                    }

                    studentAnswers.Add(new Student_Answers
                    {
                        Question_ID = question.Question_ID,
                        Selected_Option = studentAnswer,
                        Is_Correct = isCorrect
                        // Result_ID1 sẽ được gán sau
                    });
                }
                else
                {
                    Console.WriteLine($"No answer provided for Question ID: {question.Question_ID}");
                }
            }

            Console.WriteLine($"Total student answers to save: {studentAnswers.Count}");

            // Lưu Exam_Result trước
            var examResult = new Exam_Result
            {
                Exam_ID = examId,
                User_ID = studentId,
                CorrectAnswers = studentAnswers.Count(sa => sa.Is_Correct),
                WrongAnswers = studentAnswers.Count(sa => !sa.Is_Correct),
                Score = maxScore > 0 ? totalScore / maxScore * 10 : 0,
                End_Time = DateTime.Now
            };

            _context.ExamResult.Add(examResult);
            await _context.SaveChangesAsync(); // Lưu Exam_Result để tạo Result_ID

            // Gán Result_ID1 cho studentAnswers
            if (studentAnswers.Any())
            {
                foreach (var answer in studentAnswers)
                {
                    answer.Result_ID1 = examResult.Result_ID; // Sửa thành Result_ID1
                }

                Console.WriteLine($"Saving {studentAnswers.Count} student answers with Result_ID: {examResult.Result_ID}");
                _context.Answers.AddRange(studentAnswers); // Đã đúng context
            }
            else
            {
                Console.WriteLine("No student answers to save.");
            }

            // Lưu tất cả thay đổi
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving to database: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu kết quả bài thi.";
                return RedirectToAction("ExamList");
            }

            TempData["SuccessMessage"] = "Nộp bài thành công!";
            return RedirectToAction("ExamList", "StudentTakeExam");
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








    }
}
