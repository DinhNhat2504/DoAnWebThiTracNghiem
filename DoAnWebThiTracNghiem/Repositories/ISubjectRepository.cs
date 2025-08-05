using DoAnWebThiTracNghiem.Models;

namespace DoAnWebThiTracNghiem.Repositories
{
    public interface ISubjectRepository
    {
        Task<IEnumerable<Subject>> GetAllAsync(int roleid, int id);
        Task<Subject> GetByIdAsync(int id);
        Task AddAsync(Subject subject);
        Task UpdateAsync(Subject subject);
        Task DeleteAsync(int id);
        Task<int> CountAsync(int RoleAd, int id, string search);
        Task<IEnumerable<Subject>> GetPagedAsync(int RoleAd,int id, int page, int pageSize ,string search);
        Task<List<Subject>> GetSubjectsCreatedBetween(DateTime start, DateTime end);
        Task<int> CountAllAsync();
    }
}
