using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using DoAnWebThiTracNghiem.ViewModel;
using DoAnWebThiTracNghiem.Services;
using BCrypt.Net;

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


        // Trả về trang đăng ký tài khoản 
        public IActionResult Register()
        {
            // Lọc quyền Teacher và Student
            var roles = _Dbcontext.Roles
                .Where(r => r.Name == "Teacher" || r.Name == "Student")
                .ToList();

            ViewBag.Roles = new SelectList(
            roles.Select(r => new { r.Id, Name = r.Name == "Teacher" ? "Giáo viên" : "Học sinh/Sinh viên" }),
            "Id", "Name");
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
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
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
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        // Xử lý khi người dùng đăng nhập
        [HttpPost]
        public async Task<IActionResult> Login(Users model)
        {
            ModelState.Remove("FullName");
            ModelState.Remove("RoleId");
            if (!ModelState.IsValid)
                return View(model);

            var user = (await _Ucontext.GetAllAsync()).FirstOrDefault(u => u.Email == model.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                return View(model); // Trả lại view với lỗi
            }

            // Lưu thông tin người dùng vào Session
            HttpContext.Session.SetString("UserId", user.User_Id.ToString());
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("RoleId", user.RoleId.ToString());

            var role = _Dbcontext.Roles.FirstOrDefault(r => r.Id == user.RoleId);
            if (role != null)
            {
                HttpContext.Session.SetString("UserRole", role.Name);
                if (role.Name == "Admin")
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                if (role.Name == "Teacher")
                    return RedirectToAction("Index", "Home", new { area = "Teacher" });
                if (role.Name == "Student")
                    return RedirectToAction("Index", "Home", new { area = "Student" });
            }

            return RedirectToAction("Index", "Home");
        }

        // Trả về trang thay đổi mật khẩu 
        [HttpGet]
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePasswordPost(ChangePasswordViewModel model)
        {

            if (model.CurrentPassword == model.NewPassword)
            {
                ModelState.AddModelError("NewPassword", "Mật khẩu mới không được giống mật khẩu hiện tại.");
            }

            // Kiểm tra hợp lệ model
            if (!ModelState.IsValid)
            {
                return View("ChangePassword", model);
            }

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
            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password))
            {
                ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng.");
                return View("ChangePassword", model);
            }

            // Cập nhật mật khẩu mới
            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.UpdatedAt = DateTime.Now;
            await _Ucontext.UpdateAsync(user);
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
                    ModelState.AddModelError("Email", "Email không tồn tại.");
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
                    ModelState.AddModelError("Token", "Token không hợp lệ hoặc đã hết hạn.");
                    return View(model);
                }

                // Cập nhật mật khẩu mới
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                user.ResetPasswordToken = null;
                user.ResetPasswordTokenExpiry = null;
                await _Ucontext.UpdateAsync(user);

                TempData["Message"] = "Mật khẩu đã được đặt lại thành công.";
                return RedirectToAction("Login");
            }

            return View(model);
        }


    }
}
