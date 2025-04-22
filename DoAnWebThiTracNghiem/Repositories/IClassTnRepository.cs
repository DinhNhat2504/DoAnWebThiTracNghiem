using DoAnWebThiTracNghiem.Models;

namespace DoAnWebThiTracNghiem.Repositories
{
    public interface IClassTnRepository
    {
        Task<IEnumerable<ClassTn>> GetAllAsync(int id);
        Task<ClassTn> GetByIdAsync(int id);
        Task AddAsync(ClassTn classTn);
        Task UpdateAsync(ClassTn classTn);
        Task DeleteAsync(int id);
    }
}
