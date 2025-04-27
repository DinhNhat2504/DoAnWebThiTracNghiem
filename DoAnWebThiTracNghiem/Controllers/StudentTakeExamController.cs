using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnWebThiTracNghiem.Controllers
{
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
            int studentId = int.Parse(userId);

            // Kiểm tra xem bài thi có được giao cho lớp học mà sinh viên tham gia không
            var isAssigned = await _context.ClassExams
                .AnyAsync(ce => ce.Exam_ID == examId &&
                                _context.ClassStudents.Any(cs => cs.User_ID == studentId && cs.Class_ID == ce.ClassTNClass_Id));

            if (!isAssigned)
            {
                return Unauthorized();
            }

            // Kiểm tra xem sinh viên đã làm bài thi này chưa
            var hasTakenExam = await _context.ExamResult
                .AnyAsync(er => er.Exam_ID == examId && er.User_ID == studentId);

            if (hasTakenExam)
            {
                TempData["ErrorMessage"] = "Bạn đã làm bài thi này rồi.";
                return RedirectToAction("ViewResult", new { examId });
            }

            // Lấy thông tin bài thi và các câu hỏi, sắp xếp theo Question_Order
            var exam = await _context.Exams
                .Include(e => e.Exam_Questions.OrderBy(eq => eq.Question_Order))
                .ThenInclude(eq => eq.Question)
                .FirstOrDefaultAsync(e => e.Exam_ID == examId);

            if (exam == null)
            {
                return NotFound();
            }

            return View(exam);
        }


        [HttpPost]
        public async Task<IActionResult> SubmitExam(int examId, Dictionary<int, string> answers)
        {
            var userId = HttpContext.Session.GetString("UserId");
            int studentId = int.Parse(userId);

            // Kiểm tra xem bài thi có được giao cho lớp học mà sinh viên tham gia không
            var isAssigned = await _context.ClassExams
                .AnyAsync(ce => ce.Exam_ID == examId &&
                                _context.ClassStudents.Any(cs => cs.User_ID == studentId && cs.Class_ID == ce.ClassTNClass_Id));

            if (!isAssigned)
            {
                return Unauthorized();
            }

            // Lấy danh sách câu hỏi và đáp án đúng
            var questions = await _context.ExamQuestions
                .Where(eq => eq.Exam_ID == examId)
                .Include(eq => eq.Question)
                .OrderBy(eq => eq.Question_Order) // Sắp xếp theo thứ tự câu hỏi
                .ToListAsync();

            decimal totalScore = 0;
            decimal maxScore = 0;

            // Tạo danh sách để lưu câu trả lời của sinh viên
            var studentAnswers = new List<Student_Answers>();

            foreach (var question in questions)
            {
                maxScore += question.Points; // Tổng điểm tối đa của bài thi

                if (answers.TryGetValue(question.Question_ID, out var studentAnswer))
                {
                    // Kiểm tra đáp án đúng
                    bool isCorrect = studentAnswer == question.Question.Correct_Option;
                    if (isCorrect)
                    {
                        totalScore += question.Points; // Cộng điểm nếu đúng
                    }

                    // Lưu câu trả lời của sinh viên vào danh sách
                    studentAnswers.Add(new Student_Answers
                    {
                        Question_ID = question.Question_ID,
                        Selected_Option = studentAnswer, // Lưu câu trả lời của sinh viên
                        Is_Correct = isCorrect // Đánh dấu đúng/sai
                    });
                }
            }

            // Tính điểm tổng kết (tỷ lệ phần trăm)
            decimal finalScore = maxScore > 0 ? (totalScore / maxScore) * 100 : 0;

            // Lưu kết quả bài thi
            var examResult = new Exam_Result
            {
                Exam_ID = examId,
                User_ID = studentId,
                CorrectAnswers = studentAnswers.Count(sa => sa.Is_Correct),
                WrongAnswers = studentAnswers.Count(sa => !sa.Is_Correct),
                Score = finalScore,
                Start_Time = DateTime.Now, // Thời gian bắt đầu (có thể lưu từ trước)
                End_Time = DateTime.Now // Thời gian kết thúc
            };

            _context.ExamResult.Add(examResult);
            await _context.SaveChangesAsync(); // Lưu để lấy Result_ID

            // Cập nhật Result_ID1 cho các câu trả lời và lưu vào bảng Answers
            foreach (var answer in studentAnswers)
            {
                answer.Result_ID1 = examResult.Result_ID; // Gán Result_ID từ Exam_Result
            }

            _context.Answers.AddRange(studentAnswers);
            await _context.SaveChangesAsync(); // Lưu danh sách câu trả lời

            TempData["SuccessMessage"] = "Bạn đã nộp bài thi thành công.";
            return RedirectToAction("ExamList");
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

            // Lấy danh sách câu trả lời của sinh viên
            var studentAnswers = await _context.Answers
                .Where(sa => sa.Result_ID1 == examResult.Result_ID)
                .Include(sa => sa.Question)
                .ThenInclude(q => q.Exam_Questions)
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
