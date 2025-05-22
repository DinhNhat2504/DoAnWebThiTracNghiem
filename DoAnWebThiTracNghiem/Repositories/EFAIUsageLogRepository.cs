using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnWebThiTracNghiem.Repositories
{
    public class EFAIUsageLogRepository : IAIUsageLogRepository
    {
        private readonly AppDBContext _context;

        public EFAIUsageLogRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<AIUsageLog?> GetByUserAndDateAsync(int userId, DateTime date)
        {
            return await _context.AIUsageLog
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Date == date);
        }

        public async Task AddAsync(AIUsageLog log)
        {
            await _context.AIUsageLog.AddAsync(log);
        }

        public async Task UpdateAsync(AIUsageLog log)
        {
            _context.AIUsageLog.Update(log);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
