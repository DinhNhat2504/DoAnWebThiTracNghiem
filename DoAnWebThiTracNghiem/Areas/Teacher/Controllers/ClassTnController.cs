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
    public class ClassTnController : Controller
    {
        private readonly IClassTnRepository _classTnRepository;
        private readonly AppDBContext _context;
        public ClassTnController(IClassTnRepository classTnRepository, AppDBContext context)
        {
            _classTnRepository = classTnRepository;
            _context = context;
        }
        // Hiển thị danh sách lớp học do Teacher có id = session tạo 


        public async Task<IActionResult> Index(string search)
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);

            // Lấy danh sách lớp do giáo viên tạo, Include Subject và Creator
            var query = _context.ClassTn
                .Include(c => c.Subject)
                .Include(c => c.Creator)
                .Include(c => c.Student_Classes)
                .Where(c => c.CreatorUser_Id == teacherId);

            // Lọc theo tên lớp hoặc tên môn học nếu có search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    (c.ClassName != null && c.ClassName.Contains(search)) ||
                    (c.Subject != null && c.Subject.Subject_Name != null && c.Subject.Subject_Name.Contains(search))
                );
            }

            var classes = await query
                .Select(c => new
                {
                    Class = c,
                    StudentCount = _context.ClassStudents.Count(sc => sc.Class_ID == c.Class_Id)
                })
                .ToListAsync();

            // Cập nhật trạng thái hoạt động dựa trên sĩ số
            foreach (var item in classes)
            {
                item.Class.IsActive = item.StudentCount > 0;
            }

            // Lấy danh sách môn học của giáo viên
            var subjects = await _context.Subjects
                .Where(s => s.CreatorUser_Id == teacherId)
                .ToListAsync();

            ViewData["Subjects"] = subjects;
            ViewData["Search"] = search;

            // Truyền dữ liệu vào view
            return View(classes.Select(c => new ClassTn
            {
                Class_Id = c.Class.Class_Id,
                ClassName = c.Class.ClassName,
                Subject = c.Class.Subject,
                Creator = c.Class.Creator,
                IsActive = c.Class.IsActive,
                CreatedAt = c.Class.CreatedAt,
                UpdatedAt = c.Class.UpdatedAt,
                Student_Classes = c.Class.Student_Classes,
                SubjectId = c.Class.SubjectId,
                InviteCode = c.Class.InviteCode
            }).ToList());
        }



        public async Task<IActionResult> Details(int classId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);
            var classTn = await _context.ClassTn
                .Include(c => c.Subject)
                .Include(c => c.Creator)
                .FirstOrDefaultAsync(c => c.Class_Id == classId);

            if (classTn == null)
            {
                return NotFound();
            }

            var assignedExamIds = await _context.ClassExams
                .Where(ce => ce.ClassTNClass_Id == classId)
                .Select(ce => ce.Exam_ID)
                .ToListAsync();


            // Lọc các bài thi khả dụng theo giáo viên hiện tại
            var availableExams = await _context.Exams
        .Where(e => !assignedExamIds.Contains(e.Exam_ID) &&
                    e.Subject_ID == classTn.SubjectId &&
                    e.CreatorUser_Id == teacherId)
        .ToListAsync();

            var notifications = await _context.Notifications
                .Where(n => n.ClassTNClass_Id == classId)
                .OrderByDescending(n => n.Timestamp)
                .ToListAsync();

            var exams = await _context.ClassExams
                .Include(ec => ec.Exam)
                .Where(ec => ec.ClassTNClass_Id == classId)
                .ToListAsync();

            var students = await _context.ClassStudents
                .Include(sc => sc.User)
                .Where(sc => sc.Class_ID == classId)
                .Select(sc => sc.User)
                .ToListAsync();
            var examIds = await _context.ClassExams
                .Where(ec => ec.ClassTNClass_Id == classId)
               .Select(ec => ec.Exam_ID)
    .ToListAsync();
            var examCounts = await _context.ExamResult
    .Where(er => examIds.Contains(er.Exam_ID))
    .GroupBy(er => er.User_ID)
    .Select(g => new { UserId = g.Key, Count = g.Count() })
    .ToDictionaryAsync(x => x.UserId, x => x.Count);
            var viewModel = new ClassDetailsViewModel
            {
                Class = classTn,
                Notifications = notifications,
                Exams = exams,
                Students = students,
                AvailableExams = availableExams // Thêm danh sách bài thi khả dụng
            };
            // Lấy danh sách các môn học
            var subjects = await _context.Subjects.Where(e => e.CreatorUser_Id == teacherId).ToListAsync();
            ViewData["Subjects"] = subjects;
            ViewData["Class"] = classTn;
            ViewData["Notifications"] = notifications;
            ViewData["Exams"] = exams;
            ViewData["Students"] = students;
            ViewData["StudentExamCounts"] = examCounts;
            ViewData["AvailableExams"] = availableExams;

            return View(viewModel);
        }


        // Tạo thông báo mới
        [HttpPost]
        public async Task<IActionResult> CreateNotification(int classId, string content)
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);
            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction("Details", new { classId });
            }

            var notification = new Notification
            {
                Content = content,
                Timestamp = DateTime.Now,
                ClassTNClass_Id = classId,
                CreatorUser_Id = teacherId// Lấy ID của giáo viên hiện tại (từ session hoặc user context)
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { classId });
        }
        // Tạo lớp học mới

        [HttpPost]
        public async Task<IActionResult> Create(ClassTn classTn)
        {
            if (ModelState.IsValid)
            {
                var userID = HttpContext.Session.GetString("UserId");
                classTn.CreatorUser_Id = int.Parse(userID);
                classTn.CreatedAt = DateTime.Now;
                classTn.UpdatedAt = DateTime.Now;
                classTn.InviteCode = await GenerateUniqueInviteCode();
                await _classTnRepository.AddAsync(classTn);
                TempData["SuccessMessage"] = "Thêm lớp học thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu lỗi, nạp lại danh sách môn học và truyền lỗi về Index
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);
            var subjects = await _context.Subjects.Where(s => s.CreatorUser_Id == teacherId).ToListAsync();
            ViewData["Subjects"] = subjects;

            // Lấy lại danh sách lớp để truyền về Index
            var classes = await _context.ClassTn
                .Include(c => c.Subject)
                .Include(c => c.Creator)
                .Include(c => c.Student_Classes)
                .Where(c => c.CreatorUser_Id == teacherId)
                .ToListAsync();

            // Truyền ModelState lỗi về Index
            TempData["ErrorMessage"] = "Có lỗi xảy ra. Vui lòng kiểm tra lại thông tin.";
            return View("Index", classes);
        }

        // Chỉnh sửa lớp học
        public async Task<IActionResult> Edit(int id)
        {
            var classTn = await _classTnRepository.GetByIdAsync(id);
            if (classTn == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetString("UserId");
            if (classTn.CreatorUser_Id != int.Parse(userId))
            {
                return Unauthorized();
            }

            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Subject_Id", "Subject_Name", classTn.SubjectId);
            return View(classTn);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, ClassTn classTn)
        {
            if (id != classTn.Class_Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetString("UserId");
                var existingClass = await _classTnRepository.GetByIdAsync(id);
                if (existingClass == null)
                {
                    return NotFound();
                }

                if (existingClass.CreatorUser_Id != int.Parse(userId))
                {
                    return Unauthorized();
                }

                // Kiểm tra nếu môn học thay đổi
                if (existingClass.SubjectId != classTn.SubjectId)
                {
                    // Lấy danh sách các bài thi liên quan đến môn học cũ
                    var examsToDelete = await _context.ClassExams
                        .Include(ce => ce.Exam)
                        .Where(ce => ce.ClassTNClass_Id == id && ce.Exam.Subject_ID == existingClass.SubjectId)
                        .ToListAsync();

                    // Xóa các bài thi
                    _context.ClassExams.RemoveRange(examsToDelete);
                    await _context.SaveChangesAsync();
                }

                // Cập nhật các trường
                existingClass.ClassName = classTn.ClassName;
                existingClass.SubjectId = classTn.SubjectId;
                existingClass.UpdatedAt = DateTime.Now;

                await _classTnRepository.UpdateAsync(existingClass);

                TempData["SuccessMessage"] = "Cập nhật lớp học thành công!";
                return RedirectToAction("Details", new { classId = id });
            }

            // Nếu ModelState không hợp lệ, hiển thị lại trang với thông báo lỗi
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật lớp học. Vui lòng kiểm tra lại thông tin.";
            ViewData["Subjects"] = _context.Subjects.ToList();
            return View("Details", classTn);
        }




        // Xóa lớp học
        public async Task<IActionResult> Delete(int id)
        {
            var classTn = await _classTnRepository.GetByIdAsync(id);
            if (classTn == null)
            {
                return NotFound();
            }
            var userId = HttpContext.Session.GetString("UserId");
            if (classTn.CreatorUser_Id != int.Parse(userId))
            {
                return Unauthorized();
            }
            return View(classTn);
        }
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var classTn = await _classTnRepository.GetByIdAsync(id);
                if (classTn == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy lớp học để xóa.";
                    return RedirectToAction(nameof(Index));
                }

                await _classTnRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = "Xóa lớp học thành công.";
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa lớp học.";
                Console.WriteLine(e);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<string> GenerateUniqueInviteCode()
        {
            string inviteCode;
            bool exists;

            do
            {
                // Tạo mã InviteCode gồm 7 ký tự chữ hoa và số
                inviteCode = GenerateRandomCode(7);

                // Kiểm tra xem mã này đã tồn tại trong cơ sở dữ liệu chưa
                exists = await _context.ClassTn.AnyAsync(c => c.InviteCode == inviteCode);
            } while (exists);

            return inviteCode;
        }
        [HttpPost]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Details", new { classId = notification.ClassTNClass_Id });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteExam(int id)
        {
            var examClass = await _context.ClassExams.FindAsync(id);
            if (examClass != null)
            {
                // Xóa bài thi khỏi lớp
                _context.ClassExams.Remove(examClass);
                await _context.SaveChangesAsync();

                // Kiểm tra xem bài thi có còn được giao cho lớp nào khác không
                var isAssignedToOtherClasses = await _context.ClassExams
                    .AnyAsync(ce => ce.Exam_ID == examClass.Exam_ID);

                if (!isAssignedToOtherClasses)
                {
                    // Nếu không còn được giao, cập nhật IsActive = false
                    var exam = await _context.Exams.FindAsync(examClass.Exam_ID);
                    if (exam != null)
                    {
                        exam.IsActive = false;
                        _context.Exams.Update(exam);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            return RedirectToAction("Details", new { classId = examClass.ClassTNClass_Id });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var studentClass = await _context.ClassStudents.FirstOrDefaultAsync(sc => sc.User_ID == id);
            if (studentClass != null)
            {
                _context.ClassStudents.Remove(studentClass);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Details", new { classId = studentClass.Class_ID });
        }
        // Giao bài thi cho lớp
        [HttpPost]
        public async Task<IActionResult> AssignExams(int classId, List<int> selectedExams)
        {
            if (selectedExams == null || !selectedExams.Any())
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một bài thi.";
                return RedirectToAction("Details", new { classId });
            }

            foreach (var examId in selectedExams)
            {
                // Kiểm tra trạng thái IsActive của bài thi
                var exam = await _context.Exams.FindAsync(examId);
                if (exam != null && !exam.IsActive)
                {
                    exam.IsActive = true; // Cập nhật trạng thái thành hoạt động
                    _context.Exams.Update(exam);
                }

                // Giao bài thi cho lớp
                var classExam = new Exam_Class
                {
                    ClassTNClass_Id = classId,
                    Exam_ID = examId,
                    AssignedAt = DateTime.Now
                };
                _context.ClassExams.Add(classExam);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Giao bài thành công.";
            return RedirectToAction("Details", new { classId });
        }


        private string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}
