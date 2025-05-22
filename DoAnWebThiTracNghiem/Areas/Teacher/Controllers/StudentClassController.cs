using DoAnWebThiTracNghiem.Areas.Teacher.Models;
using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using System.Drawing;

namespace DoAnWebThiTracNghiem.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    public class StudentClassController : Controller
    {
        private readonly AppDBContext _context;

        public StudentClassController(AppDBContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> ListStudents(int classId, string search = "", string sortOrder = "")
        {

            // Lấy danh sách sinh viên trong lớp, áp dụng tìm kiếm nếu có
            var studentsInClassQuery = _context.ClassStudents
                .Where(sc => sc.Class_ID == classId)
                .Join(_context.Users, sc => sc.User_ID, u => u.User_Id, (sc, u) => new { sc, u });

            if (!string.IsNullOrEmpty(search))
            {
                var loweredSearch = search.ToLower();
                studentsInClassQuery = studentsInClassQuery
                    .Where(x =>
                        (x.u.FullName != null && x.u.FullName.ToLower().Contains(loweredSearch)) ||
                        (x.u.Email != null && x.u.Email.ToLower().Contains(loweredSearch))
                    );
            }

            var studentsInClass = await studentsInClassQuery.ToListAsync();

            // Lấy các bài thi đã giao cho lớp này
            var examIds = await _context.ClassExams
                .Where(ec => ec.ClassTNClass_Id == classId)
                .Select(ec => ec.Exam_ID)
                .ToListAsync();

            var viewModelList = new List<StudentInClassViewModel>();

            foreach (var item in studentsInClass)
            {
                var studentId = item.u.User_Id;
                var studentClassId = item.sc.Class_ID;

                // Kết quả thi của sinh viên này cho các bài thi đã giao
                var studentExamResults = await _context.ExamResult
                    .Where(er => er.User_ID == studentId && examIds.Contains(er.Exam_ID))
                    .ToListAsync();

                int examsCompleted = studentExamResults.Count;
                int examsPassed = studentExamResults.Count(er =>
                {
                    var exam = _context.Exams.FirstOrDefault(e => e.Exam_ID == er.Exam_ID);
                    return exam != null && er.Score >= (decimal)exam.PassScore;
                });
                int examsPending = examIds.Count - examsCompleted;

                viewModelList.Add(new StudentInClassViewModel
                {
                    StudentId = studentId,
                    ClassId = studentClassId,
                    FullName = item.u.FullName ?? "",
                    Email = item.u.Email ?? "",
                    ExamsCompleted = examsCompleted,
                    ExamsPending = examsPending,
                    ExamsPassed = examsPassed,
                    JoinedAt = item.sc.Timestamp
                });
            }

            // Sắp xếp theo yêu cầu
            switch (sortOrder)
            {
                case "completed_desc":
                    viewModelList = viewModelList.OrderByDescending(s => s.ExamsCompleted).ToList();
                    break;
                case "completed_asc":
                    viewModelList = viewModelList.OrderBy(s => s.ExamsCompleted).ToList();
                    break;
                case "pending_desc":
                    viewModelList = viewModelList.OrderByDescending(s => s.ExamsPending).ToList();
                    break;
                case "pending_asc":
                    viewModelList = viewModelList.OrderBy(s => s.ExamsPending).ToList();
                    break;
                case "passed_desc":
                    viewModelList = viewModelList.OrderByDescending(s => s.ExamsPassed).ToList();
                    break;
                case "passed_asc":
                    viewModelList = viewModelList.OrderBy(s => s.ExamsPassed).ToList();
                    break;
                default:
                    break;
            }
            ViewBag.Search = search;
            ViewBag.SortOrder = sortOrder;
            ViewBag.ClassId = classId;
            return View(viewModelList);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveStudent(int classId, int studentId)
        {
            var studentClass = await _context.ClassStudents
                .FirstOrDefaultAsync(sc => sc.Class_ID == classId && sc.User_ID == studentId);

            if (studentClass != null)
            {
                _context.ClassStudents.Remove(studentClass);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa sinh viên khỏi lớp.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy sinh viên trong lớp.";
            }

            return RedirectToAction(nameof(ListStudents), new { classId });
        }
        public async Task<IActionResult> StudentExamResults(int classId, int studentId)
        {
            // Lấy các bài thi đã giao cho lớp này
            var examIds = await _context.ClassExams
                .Where(ec => ec.ClassTNClass_Id == classId)
                .Select(ec => ec.Exam_ID)
                .ToListAsync();

            // Lấy kết quả thi của sinh viên này cho các bài thi đã giao
            var results = await _context.ExamResult
                .Where(er => er.User_ID == studentId && examIds.Contains(er.Exam_ID))
                .Include(er => er.Exam)
                .ToListAsync();

            var student = await _context.Users.FindAsync(studentId);

            return View("StudentExamResults", new StudentExamResultsViewModel
            {
                Student = student,
                Results = results
            });
        }

        public async Task<IActionResult> ExamResults(
            int examId,
            int classId,
            string search = "",
            string passFilter = "",
            string sortOrder = "")
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);

            // Get student IDs in class
            var studentIdsInClass = await _context.ClassStudents
                .Where(sc => sc.Class_ID == classId)
                .Select(sc => sc.User_ID)
                .ToListAsync();

            // Get exam and pass score
            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Exam_ID == examId && e.CreatorUser_Id == teacherId);
            if (exam == null) return NotFound();

            double passScore = exam.PassScore;

            // Get results
            var resultsQuery = _context.ExamResult
     .Where(r => r.Exam_ID == examId
                 && r.Exam.CreatorUser_Id == teacherId
                 && studentIdsInClass.Contains(r.User_ID))
     .Include(r => r.User)
     .Include(r => r.Exam)
     .Include(r => r.Student_Answers) // BỔ SUNG DÒNG NÀY
     .AsQueryable();


            // Search by student name
            if (!string.IsNullOrEmpty(search))
            {
                resultsQuery = resultsQuery.Where(r => r.User.FullName.Contains(search));
            }

            // Filter by pass/fail
            if (passFilter == "pass")
            {
                resultsQuery = resultsQuery.Where(r => r.Score >= (decimal)passScore);
            }
            else if (passFilter == "fail")
            {
                resultsQuery = resultsQuery.Where(r => r.Score < (decimal)passScore);
            }

            // Sorting
            if (sortOrder == "score_desc")
            {
                resultsQuery = resultsQuery.OrderByDescending(r => r.Score);
            }
            else if (sortOrder == "score_asc")
            {
                resultsQuery = resultsQuery.OrderBy(r => r.Score);
            }
            else
            {
                resultsQuery = resultsQuery.OrderBy(r => r.User.FullName);
            }

            var results = await resultsQuery.ToListAsync();

            var viewModel = new ExamResultsViewModel
            {
                ExamName = exam.Exam_Name,
                PassScore = (decimal)passScore,
                Results = results.Select(r => new ExamResultListItemViewModel
                {
                    ResultId = r.Result_ID,
                    StudentName = r.User.FullName,
                    Score = r.Score,
                    IsPassed = r.Score >= (decimal)passScore,
                    CorrectAnswers = r.Student_Answers?.Count(sa => sa.Is_Correct) ?? 0,
                    WrongAnswers = r.Student_Answers?.Count(sa => !sa.Is_Correct) ?? 0,

                }).ToList(),
                Search = search,
                PassFilter = passFilter,
                SortOrder = sortOrder
            };

            ViewBag.ClassId = classId;
            ViewBag.ExamId = examId;
            return View(viewModel);
        }

        // GET: Teacher/StudentClass/ExamResultDetail/5
        public async Task<IActionResult> ExamResultDetail(int resultId)
        {
            var result = await _context.ExamResult
                .Include(r => r.User)
                .Include(r => r.Exam)
                .Include(r => r.Student_Answers)
                    .ThenInclude(sa => sa.Question)
                .FirstOrDefaultAsync(r => r.Result_ID == resultId);

            if (result == null)
            {
                return NotFound();
            }

            return View(result);
        }


        [HttpGet]
        public async Task<IActionResult> ExportExcel(int examId, int classId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);

            // Lấy danh sách sinh viên trong lớp
            var studentIdsInClass = await _context.ClassStudents
                .Where(sc => sc.Class_ID == classId)
                .Select(sc => sc.User_ID)
                .ToListAsync();

            // Lấy thông tin bài thi
            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Exam_ID == examId && e.CreatorUser_Id == teacherId);
            if (exam == null) return NotFound();

            // Lấy kết quả thi
            var results = await _context.ExamResult
                .Where(r => r.Exam_ID == examId && studentIdsInClass.Contains(r.User_ID))
                .Include(r => r.User)
                .ToListAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Kết quả thi");

            // Header
            ws.Cells[1, 1].Value = "STT";
            ws.Cells[1, 2].Value = "Họ tên";
            ws.Cells[1, 3].Value = "Email";
            ws.Cells[1, 4].Value = "Điểm";
            ws.Cells[1, 5].Value = "Đạt";
            ws.Cells[1, 6].Value = "Số câu đúng";
            ws.Cells[1, 7].Value = "Số câu sai";
            ws.Cells[1, 8].Value = "Thời gian bắt đầu";
            ws.Cells[1, 9].Value = "Thời gian kết thúc";

            using (var range = ws.Cells[1, 1, 1, 9])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            }

            int row = 2;
            int stt = 1;
            foreach (var r in results)
            {
                ws.Cells[row, 1].Value = stt++;
                ws.Cells[row, 2].Value = r.User?.FullName ?? "";
                ws.Cells[row, 3].Value = r.User?.Email ?? "";
                ws.Cells[row, 4].Value = r.Score;
                ws.Cells[row, 5].Value = r.Score >= (decimal)exam.PassScore ? "Đạt" : "Không đạt";
                ws.Cells[row, 6].Value = r.CorrectAnswers;
                ws.Cells[row, 7].Value = r.WrongAnswers;
                ws.Cells[row, 8].Value = r.Start_Time.ToString("dd/MM/yyyy HH:mm");
                ws.Cells[row, 9].Value = r.End_Time.ToString("dd/MM/yyyy HH:mm");
                row++;
            }

            ws.Cells[ws.Dimension.Address].AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            string fileName = $"KetQua_{exam.Exam_Name}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }
}