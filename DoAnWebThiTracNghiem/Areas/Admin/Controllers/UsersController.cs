using DoAnWebThiTracNghiem.Areas.Admin.Models;
using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.Repositories;
using DoAnWebThiTracNghiem.ViewModel;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace DoAnWebThiTracNghiem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController : Controller
    {
        private readonly IUserRepository _Ucontext;
        private readonly AppDBContext _context;

        public UsersController(IUserRepository ucontext, AppDBContext context)
        {
            _Ucontext = ucontext;
            _context = context;
        }
        public async Task<IActionResult> Index(int page = 1, int pageSize = 5 , string search ="")
        {
            var totalUsers = await _Ucontext.CountAsync(search);
            var users = await _Ucontext.GetPagedAsync(page, pageSize,search);
            var roles= await _Ucontext.GetAllRoleAsync();

            var userIds = users.Select(u => u.User_Id).ToList();
            var roleIds = users.Select(u => u.RoleId).ToList();
            var activityCounts = await _Ucontext.GetUserActivityCountsAsync(userIds, roleIds);

            var activityList = users.Select(u =>
            {
                var activity = activityCounts.FirstOrDefault(a => a.UserId == u.User_Id) ?? new UserActivityCountViewModel();
                return new UserActivityViewModel
                {
                    User = u,
                    ExamCount = activity.ExamCount,
                    SubjectCount = activity.SubjectCount,
                    QuestionCount = activity.QuestionCount,
                    ClassCount = activity.ClassCount,
                    JoinedClassCount = activity.JoinedClassCount,
                    TakenExamCount = activity.TakenExamCount
                };
            }).ToList();
            var model = new PagedResult<UserActivityViewModel>
            {
                Items = activityList,
                PageNumber = page,
                TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize),
                TotalItems = totalUsers,
                Search = search
            };
            // Lấy danh sách vai trò để hiển thị trong dropdown
            ViewData["Roles"] = roles;
            return View(model);
        }


        // File: Areas/Admin/Controllers/UsersController.cs
        public async Task<IActionResult> Details(int id)
        {
            var user = await _Ucontext.GetByIdAsync(id);
            if (user == null) return NotFound();

            var vm = new UserDetailViewModel { User = user };

            var role = user.Role?.Name?.ToLower();

            if (role == "teacher")
            {
                vm.CreatedClassCount = await _context.ClassTn.CountAsync(c => c.CreatorUser_Id == id);
                vm.CreatedExamCount = await _context.Exams.CountAsync(e => e.CreatorUser_Id == id);
                vm.CreatedSubjectCount = await _context.Subjects.CountAsync(s => s.CreatorUser_Id == id);
                vm.CreatedQuestionCount = await _context.Question.CountAsync(q => q.CreatorUser_Id == id);
            }
            else if (role == "student")
            {
                vm.JoinedClassCount = await _context.ClassStudents.CountAsync(sc => sc.User_ID == id);
                vm.TakenExamCount = await _context.ExamResult.CountAsync(er => er.User_ID == id);
            }
            // Admin: không cần thống kê gì thêm

            return View(vm);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Users user)
        {
            bool emailExists = await _Ucontext.ExistsByEmailAsync(user.Email);

            if (emailExists)
            {
                return Json(new { success = false, message = "Email đã tồn tại trong hệ thống." });
            }

            if (ModelState.IsValid)
            {
                user.CreatedAt = DateTime.Now;
                user.UpdatedAt = DateTime.Now;
                await _Ucontext.AddAsync(user);
                // Trả về JSON thành công
                return Json(new { success = true, message = "Thêm người dùng thành công !" });
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
        [HttpGet]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _Ucontext.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Json(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditInline( Users user, IFormFile AvatarFile)
        {
            if (user == null) return BadRequest();
            ModelState.Remove("RoleId");
            ModelState.Remove("AvatarFile");

            // Kiểm tra hợp lệ dữ liệu
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

            var existing = await _Ucontext.GetByIdAsync(user.User_Id);
            if (existing == null) return NotFound();

            existing.FullName = user.FullName;
            existing.Email = user.Email;
            if (!string.IsNullOrEmpty(user.Password))
            {
                // Mã hóa mật khẩu mới nếu có nhập
                existing.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            }
            existing.PhoneNumber = user.PhoneNumber;
            existing.Address = user.Address;
            existing.UpdatedAt = DateTime.Now;

            // Xử lý upload avatar
            if (AvatarFile != null && AvatarFile.Length > 0)
            {
                existing.AvatarUrl = await SaveImage(AvatarFile);
            }

            await _Ucontext.UpdateAsync(existing);
            return Json(new { success = true, message = "Sửa người dùng thành công!" });
        }

        // Lưu hình ảnh vào thư mục wwwroot/images
        private async Task<string> SaveImage(IFormFile image)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var savePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + fileName;
        }


        [HttpPost]
        public async Task<IActionResult> DeleteInline(int id)
        {
            var user = await _Ucontext.GetByIdAsync(id);
            if (user == null) return NotFound();
            if (user.Role?.Name?.ToLower() == "admin") return BadRequest("Không thể xóa admin!");
            await _Ucontext.DeleteAsync(id);
            return Json(new { success = true, message = "Xóa người dùng thành công!" });
        }

    }
}
