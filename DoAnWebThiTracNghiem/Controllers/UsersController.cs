using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using DoAnWebThiTracNghiem.ViewModel;
using DoAnWebThiTracNghiem.Services;

namespace DoAnWebThiTracNghiem.Controllers
{
    public class UsersController : Controller

    {
        private readonly IUserRepository _Ucontext;
        private readonly AppDBContext _Dbcontext;
        private readonly EmailService _emailService;

        // Hàm khởi tạo các biến cần thiết 
        public UsersController(IUserRepository Ucontext, AppDBContext Dbcontext, EmailService emailService)
        {
            _Ucontext = Ucontext;
            _Dbcontext = Dbcontext;
            _emailService = emailService;
        }
        // Trả về Index 
        public async Task<IActionResult> Index()
        {
            var users = await _Ucontext.GetAllAsync();
            return View(users);
        }
        // Trả về trang chi tiết người dùng
        public async Task<IActionResult> Details(int id)
        {
            var user = await _Ucontext.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
        // Trả về trang thêm mới người dùng
        public IActionResult Create()
        {
            ViewData["RoleId"] = new SelectList(_Dbcontext.Roles, "Id", "Name");
            return View();
        }
        // Xử lý khi người dùng thêm mới người dùng
        [HttpPost]
        public async Task<IActionResult> Create(Users user, IFormFile avatarUrl)
        {
            
                if (ModelState.IsValid)
                {
                    if (avatarUrl != null)
                    {
                        user.AvatarUrl = await SaveImage(avatarUrl);
                    }

                    await _Ucontext.AddAsync(user);
                    return RedirectToAction(nameof(Index));
                }
           

            ViewData["RoleId"] = new SelectList(_Dbcontext.Roles, "Id", "Name", user.RoleId);
            return View(user);
        }
        // Lưu hình ảnh vào thư mục wwwroot/images
        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName);
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName;
        }
        // Trả về trang đăng ký tài khoản 
        public IActionResult Register()
        {
            // Lọc quyền Teacher và Student
            var roles = _Dbcontext.Roles
                .Where(r => r.Name == "Teacher" || r.Name == "Student")
                .ToList();

            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View();
        }

        // Xử lý khi người dùng đăng ký tài khoản
        [HttpPost]
        public async Task<IActionResult> Register(Users user)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra email đã tồn tại chưa
                var existingUser = await _Ucontext.GetAllAsync();
                if (existingUser.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng.");
                    var roles = _Dbcontext.Roles
                        .Where(r => r.Name == "Teacher" || r.Name == "Student")
                        .ToList();
                    ViewBag.Roles = new SelectList(roles, "Id", "Name");
                    return View(user);
                }

                // Lưu thông tin người dùng
                user.CreatedAt = DateTime.Now;
                user.UpdatedAt = DateTime.Now;

                await _Ucontext.AddAsync(user);


                return RedirectToAction("Login");
            }

            var filteredRoles = _Dbcontext.Roles
                .Where(r => r.Name == "Teacher" || r.Name == "Student")
                .ToList();
            ViewBag.Roles = new SelectList(filteredRoles, "Id", "Name");
            return View(user);
        }

         // Trả về trang đăng nhập
        public IActionResult Login()
        {
            return View();
        }
         // Xử lý khi người dùng đăng nhập
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = (await _Ucontext.GetAllAsync()).FirstOrDefault(u => u.Email == email);
            if (user == null || user.Password != password)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                return View();
            }

            // Lưu thông tin người dùng vào Session
            HttpContext.Session.SetString("UserId", user.User_Id.ToString());
            HttpContext.Session.SetString("UserName", user.FullName);

            // Lưu quyền vào Session
            var role = _Dbcontext.Roles.FirstOrDefault(r => r.Id == user.RoleId);
            if (role != null)
            {
                HttpContext.Session.SetString("UserRole", role.Name);
                // Chuyển hướng dựa trên quyền
                if (role.Name == "Admin")
                {
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }
                else if (role.Name == "Teacher")
                {
                    return RedirectToAction("Index", "Home", new { area = "Teacher" });
                }
            }

            return RedirectToAction("Index", "Home");
        }

        // Trả về trang thay đổi mật khẩu 
        public IActionResult ChangePassword()
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToAction("Login"); // Chuyển đến trang Login nếu người dùng chưa đăng nhập
            }

            return View();
        }
        // Xử lý khi người dùng thay đổi mật khẩu
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            // Lấy UserId từ session
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            // Lấy thông tin người dùng từ cơ sở dữ liệu
            var user = await _Ucontext.GetByIdAsync(int.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }

            // Kiểm tra mật khẩu hiện tại
            if (user.Password != currentPassword) // So sánh trực tiếp mật khẩu
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu hiện tại không đúng.");
                return View();
            }

            // Cập nhật mật khẩu mới
            user.Password = newPassword; // Lưu trực tiếp mật khẩu mới
            user.UpdatedAt = DateTime.Now;

            await _Ucontext.UpdateAsync(user);

            TempData["Message"] = "Mật khẩu đã được thay đổi thành công.";
            return RedirectToAction("Index", "Home");
        }
        // Đăng xuất
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        public IActionResult ProtectedAction()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToAction("Login");
            }

            // Logic của action
            return View();
        }
        // Xử lý khi người dùng xem thông tin cá nhân
        public async Task<IActionResult> Profile()
        {
            // Lấy UserId từ session
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            // Lấy thông tin người dùng từ cơ sở dữ liệu
            var user = await _Ucontext.GetByIdAsync(int.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        // Xử lý khi người dùng cập nhật thông tin cá nhân
        [HttpPost]
        public async Task<IActionResult> Profile(Users user , IFormFile avatarUrl)
        {
            ModelState.Remove("AvatarUrl");
            if (ModelState.IsValid)
            {
                // Lấy UserId từ session
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId) || user.User_Id != int.Parse(userId))
                {
                    return Unauthorized();
                }

                // Cập nhật thông tin người dùng
                var existingUser = await _Ucontext.GetByIdAsync(user.User_Id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                existingUser.FullName = user.FullName;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.Address = user.Address;
                existingUser.UpdatedAt = DateTime.Now;

                if (avatarUrl == null)
                {
                    user.AvatarUrl = existingUser.AvatarUrl;
                     
                }
                else
                {
                    // Lưu hình ảnh mới
                    user.AvatarUrl = await SaveImage(avatarUrl);
                    existingUser.AvatarUrl = user.AvatarUrl;
                }

                await _Ucontext.UpdateAsync(existingUser);
                TempData["Message"] = "Thông tin cá nhân đã được cập nhật thành công.";
                return RedirectToAction(nameof(Profile));
            }

            return View(user);
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = (await _Ucontext.GetAllAsync()).FirstOrDefault(u => u.Email == model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Email không tồn tại.");
                    return View(model);
                }

                // Tạo token đặt lại mật khẩu
                user.ResetPasswordToken = Guid.NewGuid().ToString();
                user.ResetPasswordTokenExpiry = DateTime.Now.AddHours(1); // Token có hiệu lực trong 1 giờ
                await _Ucontext.UpdateAsync(user);

                // Gửi email chứa link đặt lại mật khẩu
                var resetLink = Url.Action("ResetPassword", "Users", new { token = user.ResetPasswordToken }, Request.Scheme);
                var emailBody = $"<p>Click vào link sau để đặt lại mật khẩu:</p><a href='{resetLink}'>Đặt lại mật khẩu</a>";

                await _emailService.SendEmailAsync(user.Email, "Đặt lại mật khẩu", emailBody);

                TempData["Message"] = "Hướng dẫn đặt lại mật khẩu đã được gửi đến email của bạn.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // Hàm gửi email
        private async Task SendResetPasswordEmail(string email, string resetLink)
        {
            var subject = "Đặt lại mật khẩu";
            var body = $"<p>Click vào link sau để đặt lại mật khẩu:</p><a href='{resetLink}'>Đặt lại mật khẩu</a>";

            // Sử dụng dịch vụ email 
            await _emailService.SendEmailAsync(email, subject, body);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token không hợp lệ.");
            }

            return View(new ResetPasswordViewModel { Token = token });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = (await _Ucontext.GetAllAsync()).FirstOrDefault(u => u.ResetPasswordToken == model.Token && u.ResetPasswordTokenExpiry > DateTime.Now);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Token không hợp lệ hoặc đã hết hạn.");
                    return View(model);
                }

                // Cập nhật mật khẩu mới
                user.Password = model.NewPassword;
                user.ResetPasswordToken = null; // Xóa token sau khi sử dụng
                user.ResetPasswordTokenExpiry = null;
                await _Ucontext.UpdateAsync(user);

                TempData["Message"] = "Mật khẩu đã được đặt lại thành công.";
                return RedirectToAction("Login");
            }

            return View(model);
        }


    }
}
