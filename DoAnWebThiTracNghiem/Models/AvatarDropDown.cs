using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWebThiTracNghiem.Models
{
   
    public class AvatarDropDown : ViewComponent
    {
        private readonly IUserRepository _Ucontext;
        private readonly AppDBContext _Dbcontext;


        // Hàm khởi tạo các biến cần thiết 
        public AvatarDropDown(IUserRepository Ucontext, AppDBContext Dbcontext)
        {
            _Ucontext = Ucontext;
            _Dbcontext = Dbcontext;

        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Lấy UserId từ session
            var userId = HttpContext.Session.GetString("UserId");

            // Lấy thông tin người dùng từ cơ sở dữ liệu
            var user = await _Ucontext.GetByIdAsync(int.Parse(userId));

            return View(user);
        }

    }
}
