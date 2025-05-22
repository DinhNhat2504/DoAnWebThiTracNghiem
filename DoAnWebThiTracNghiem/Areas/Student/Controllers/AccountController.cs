using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnWebThiTracNghiem.Areas.Student.Controllers
{
    [Area("Student")]
    public class AccountController : Controller
    {
        private readonly AppDBContext _context;
        public AccountController(AppDBContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index(int? classId)
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId"));
            var user = await _context.Users.FindAsync(userId);

            // Lấy các lớp đã tham gia
            var classIds = await _context.ClassStudents
                .Where(sc => sc.User_ID == userId)
                .Select(sc => sc.Class_ID)
                .ToListAsync();

            var classes = await _context.ClassTn
                .Where(c => classIds.Contains(c.Class_Id))
                .ToListAsync();

            // Lọc theo lớp (nếu có)
            int selectedClassId = classId ?? classes.FirstOrDefault()?.Class_Id ?? 0;

            // Lấy thành tích từng lớp
            var classAchievements = new List<ClassAchievement>();
            foreach (var c in classes)
            {
                var examIds = await _context.ClassExams
                    .Where(ec => ec.ClassTNClass_Id == c.Class_Id)
                    .Select(ec => ec.Exam_ID)
                    .ToListAsync();

                var results = await _context.ExamResult
                    .Where(er => er.User_ID == userId && examIds.Contains(er.Exam_ID))
                    .Include(er => er.Exam)
                    .ToListAsync();

                classAchievements.Add(new ClassAchievement
                {
                    ClassId = c.Class_Id,
                    ClassName = c.ClassName,
                    ExamsTaken = results.Count,
                    AverageScore = results.Count > 0 ? Math.Round(results.Average(r => (double)r.Score), 2) : 0,
                    ExamScores = results.Select(r => new ExamScoreChartPoint
                    {
                        ExamName = r.Exam.Exam_Name,
                        Score = (double)r.Score
                    }).ToList()
                });
            }

            var vm = new StudentProfileViewModel
            {
                UserId = userId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                Address = user.Address,
                AvatarUrl = user.AvatarUrl,
                TotalClasses = classes.Count,
                Classes = classes,
                ClassAchievements = classAchievements,
                SelectedClassId = selectedClassId
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(StudentProfileViewModel model)
        {
            var user = await _context.Users.FindAsync(model.UserId);
            if (user != null)
            {
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.PhoneNumber = model.Phone;
                user.Address = model.Address;

                if (model.AvatarFile != null && model.AvatarFile.Length > 0)
                {
                    user.AvatarUrl = await SaveImage(model.AvatarFile);
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName);
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName;
        }

    }
}
