using DoAnWebThiTracNghiem.Models;

namespace DoAnWebThiTracNghiem.Repositories
{
    public interface IAIUsageLogRepository
    {
        Task<AIUsageLog?> GetByUserAndDateAsync(int userId, DateTime date);
        Task AddAsync(AIUsageLog log);
        Task UpdateAsync(AIUsageLog log);
        Task SaveChangesAsync();
    }
}
