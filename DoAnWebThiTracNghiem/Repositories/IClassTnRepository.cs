using DoAnWebThiTracNghiem.Models;

namespace DoAnWebThiTracNghiem.Repositories
{
    public interface IClassTnRepository
    {
        Task<IEnumerable<ClassTn>> GetAllAsync(int RoleAd, int id);
        Task<ClassTn> GetByIdAsync(int id);
        Task AddAsync(ClassTn classTn);
        Task UpdateAsync(ClassTn classTn);
        Task DeleteAsync(int id);
        Task<int> CountAsync(int RoleAd,int id,string search);
        Task<IEnumerable<ClassTn>> GetPagedAsync(int RoleAd,int id, int page, int pageSize, string search);
        Task<List<Subject>> GetAllSubjectsAsync(int RoleAd, int id);
        Task<List<Subject>> GetSubjectsByCreatorAsync(int creatorUserId);
    }
}
