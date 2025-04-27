using DoAnWebThiTracNghiem.Models;

namespace DoAnWebThiTracNghiem.Repositories
{
    public interface IExamRepository
    {
        Task<IEnumerable<Exam>> GetAllAsync(int id);
        Task<Exam> GetByIdAsync(int id);
        Task AddAsync(Exam exam);
        Task UpdateAsync(Exam exam);
        Task DeleteAsync(int id);
    }
}
