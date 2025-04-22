using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWebThiTracNghiem.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        [Area("Admin")]
        
        public IActionResult Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole) || userRole != "Admin")
            {
                return RedirectToAction("Login", "Users", new { area = "" });
            }

            return View();
        }
    }
}
