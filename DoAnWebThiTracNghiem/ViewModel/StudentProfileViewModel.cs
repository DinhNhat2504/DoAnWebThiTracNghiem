using DoAnWebThiTracNghiem.Models;

namespace DoAnWebThiTracNghiem.ViewModel
{
    public class StudentProfileViewModel
    {
        // Thông tin cá nhân
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
        public IFormFile? AvatarFile { get; set; }

        // Thành tích
        public int TotalClasses { get; set; }
        public List<ClassAchievement> ClassAchievements { get; set; } = new();
        public int SelectedClassId { get; set; }
        public List<ClassTn> Classes { get; set; } = new();
    }

    public class ClassAchievement
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int ExamsTaken { get; set; }
        public double AverageScore { get; set; }
        public List<ExamScoreChartPoint> ExamScores { get; set; } = new();
    }

    public class ExamScoreChartPoint
    {
        public string ExamName { get; set; }
        public double Score { get; set; }
    }
}
