using Microsoft.AspNetCore.Mvc;

namespace DoAnWebThiTracNghiem.Areas.Student.Controllers
{
    public class StudentExamController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
