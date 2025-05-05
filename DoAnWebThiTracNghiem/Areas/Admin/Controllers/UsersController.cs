using DoAnWebThiTracNghiem.Areas.Admin.Models;
using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.Repositories;
using DoAnWebThiTracNghiem.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> Index(int page = 1, int pageSize = 5)
        {
            var totalUsers = await _Ucontext.CountAsync();
            var users = await _Ucontext.GetPagedAsync(page, pageSize);

            var model = new PagedResult<Users>
            {
                Items = users,
                PageNumber = page,
                TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize),
                TotalItems = totalUsers
            };

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


        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Users user)
        {
            if (ModelState.IsValid)
            {
                await _Ucontext.AddAsync(user);
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }
        [HttpGet]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _Ucontext.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Json(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditInline(Users user, IFormFile AvatarFile)
        {
            if (user == null) return BadRequest();
            var existing = await _Ucontext.GetByIdAsync(user.User_Id);
            if (existing == null) return NotFound();

            existing.FullName = user.FullName;
            existing.Email = user.Email;
            if (!string.IsNullOrEmpty(user.Password)) existing.Password = user.Password;
            existing.PhoneNumber = user.PhoneNumber;
            existing.Address = user.Address;
            existing.UpdatedAt = DateTime.Now;

            // Xử lý upload avatar
            if (AvatarFile != null && AvatarFile.Length > 0)
            {
                existing.AvatarUrl = await SaveImage(AvatarFile);
            }

            await _Ucontext.UpdateAsync(existing);
            return Ok();
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
            return Ok();
        }

    }
}
