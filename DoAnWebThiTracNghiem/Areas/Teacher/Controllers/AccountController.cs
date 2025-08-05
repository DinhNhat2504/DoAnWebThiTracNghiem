using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.Repositories;
using DoAnWebThiTracNghiem.Services;
using DoAnWebThiTracNghiem.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWebThiTracNghiem.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    public class AccountController : Controller
    {
        private readonly IUserRepository _Ucontext;
        private readonly AppDBContext _Dbcontext;
        

        // Hàm khởi tạo các biến cần thiết 
        public AccountController(IUserRepository Ucontext, AppDBContext Dbcontext)
        {
            _Ucontext = Ucontext;
            _Dbcontext = Dbcontext;
            
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
        public async Task<IActionResult> Profile(Users user, IFormFile AvatarFile)
        {
                ModelState.Remove("AvatarUrl");

            
                // Lấy UserId từ session
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId) || user.User_Id != int.Parse(userId))
                {
                    return Unauthorized();
                }

                // Lấy thông tin người dùng hiện tại
                var existingUser = await _Ucontext.GetByIdAsync(user.User_Id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                existingUser.FullName = user.FullName;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.Address = user.Address;
                existingUser.UpdatedAt = DateTime.Now;

                // Xử lý upload avatar
                if (AvatarFile != null && AvatarFile.Length > 0)
                {
                    existingUser.AvatarUrl = await SaveImage(AvatarFile);
                }

                await _Ucontext.UpdateAsync(existingUser);
                TempData["Message"] = "Thông tin cá nhân đã được cập nhật thành công.";
                return RedirectToAction(nameof(Profile));
            

            
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

       
       
    }
}
