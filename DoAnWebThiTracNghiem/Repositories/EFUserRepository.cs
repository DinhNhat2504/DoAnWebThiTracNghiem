using DoAnWebThiTracNghiem.Areas.Admin.Models;
using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnWebThiTracNghiem.Repositories
{
    public class EFUserRepository : IUserRepository
    {
        private readonly AppDBContext _context;
        public EFUserRepository(AppDBContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Users>> GetAllAsync()
        {
            return await _context.Users.Include(u => u.Role).ToListAsync();
        }

        public async Task<IEnumerable<Roles>> GetAllRoleAsync()
        {
            return await _context.Roles.ToListAsync();
        }
        public async Task<Users> GetByIdAsync(int id)
        {
            return await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.User_Id == id);
        }
        public async Task AddAsync(Users user)
        {

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Users user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

        }
        public async Task<IEnumerable<Users>> GetPagedAsync(int page, int pageSize , string search)
        {
            return await _context.Users
                .Where(c => string.IsNullOrEmpty(search) || c.FullName.Contains(search) || c.Address.Contains(search) || c.Email.Contains(search))
                .OrderBy(u => u.User_Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(u => u.Role)
                .ToListAsync();
        }

        public async Task<int> CountAsync(string search)
        {
            return await _context.Users.Where(c => string.IsNullOrEmpty(search) || c.FullName.Contains(search) || c.Address.Contains(search) || c.Email.Contains(search)).CountAsync();
        }
        public async Task<List<Users>> GetUsersCreatedBetween(DateTime start, DateTime end)
        {
            // Đảm bảo end là cuối ngày
            var endOfDay = end.Date.AddDays(1).AddTicks(-1);
            return await _context.Users
                .Where(u => u.CreatedAt >= start.Date && u.CreatedAt <= endOfDay)
                .Include(u => u.Role) // Nếu cần lấy Role.Name
                .ToListAsync();
        }
        public async Task<List<UserActivityCountViewModel>> GetUserActivityCountsAsync(List<int> userIds, List<int> roleIds)
        {
            var examCounts = await _context.Exams
                .Where(e => userIds.Contains(e.CreatorUser_Id))
                .GroupBy(e => e.CreatorUser_Id)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToListAsync();

            var subjectCounts = await _context.Subjects
                .Where(s => userIds.Contains(s.CreatorUser_Id))
                .GroupBy(s => s.CreatorUser_Id)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToListAsync();

            var questionCounts = await _context.Question
                .Where(q => userIds.Contains(q.CreatorUser_Id))
                .GroupBy(q => q.CreatorUser_Id)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToListAsync();

            var classCounts = await _context.ClassTn
                .Where(c => userIds.Contains(c.CreatorUser_Id))
                .GroupBy(c => c.CreatorUser_Id)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToListAsync();

            // Thêm cho học sinh
            var joinedClassCounts = await _context.ClassStudents
                .Where(sc => userIds.Contains(sc.User_ID))
                .GroupBy(sc => sc.User_ID)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToListAsync();

            var takenExamCounts = await _context.ExamResult
                .Where(er => userIds.Contains(er.User_ID))
                .GroupBy(er => er.User_ID)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = userIds.Select(id => new UserActivityCountViewModel
            {
                UserId = id,
                ExamCount = examCounts.FirstOrDefault(x => x.UserId == id)?.Count ?? 0,
                SubjectCount = subjectCounts.FirstOrDefault(x => x.UserId == id)?.Count ?? 0,
                QuestionCount = questionCounts.FirstOrDefault(x => x.UserId == id)?.Count ?? 0,
                ClassCount = classCounts.FirstOrDefault(x => x.UserId == id)?.Count ?? 0,
                JoinedClassCount = joinedClassCounts.FirstOrDefault(x => x.UserId == id)?.Count ?? 0,
                TakenExamCount = takenExamCounts.FirstOrDefault(x => x.UserId == id)?.Count ?? 0
            }).ToList();

            return result;
        }
        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
        public async Task<int> CountByRoleAsync(string role)
        {
            return await _context.Users.CountAsync(u => u.Role.Name == role);
        }
        public async Task<List<Users>> GetRecentUsersAsync(int take)
        {
            return await _context.Users
        .Include(u => u.Role) 
        .OrderByDescending(u => u.CreatedAt)
        .Take(take)
        .ToListAsync();
        }

    }
}
