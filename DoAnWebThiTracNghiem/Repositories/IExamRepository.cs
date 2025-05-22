using DoAnWebThiTracNghiem.Models;

namespace DoAnWebThiTracNghiem.Repositories
{
    public interface IExamRepository
    {
        Task<IEnumerable<Exam>> GetAllAsync(int id);
        Task<Exam> GetByIdAsync(int id);
        Task<Exam> GetByIdQSAsync(int id);
        Task AddAsync(Exam exam);
        Task UpdateAsync(Exam exam);
        Task DeleteAsync(int id);
        Task<int> CountAsync(int RoleAd,int id , string search);
        Task<IEnumerable<Exam>> GetPagedAsync( int RoleAd,int id, int page, int pageSize , string search);
        Task<List<Exam>> GetExamsCreatedBetween(DateTime start, DateTime end);
        Task<int> CountAllAsync();

    }
}
