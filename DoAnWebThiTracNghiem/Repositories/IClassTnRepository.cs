using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.ViewModel;

namespace DoAnWebThiTracNghiem.Repositories
{
    public interface IClassTnRepository
    {
        Task<IEnumerable<ClassTn>> GetAllAsync(int RoleAd, int id);
        Task<ClassTn> GetByIdAsync(int id);
        Task AddAsync(ClassTn classTn);
        Task UpdateAsync(ClassTn classTn);
        Task DeleteAsync(int id);
        Task<bool> Check(string code);
        Task<int> CountAsync(int RoleAd,int id,string search);
        Task<IEnumerable<ClassTn>> GetPagedAsync(int RoleAd,int id, int page, int pageSize, string search);
        Task<List<SubjectViewModel>> GetAllSubjectsAsync(int RoleAd, int id);
        Task<List<Subject>> GetSubjectsByCreatorAsync(int creatorUserId);
        Task<List<ClassTn>> GetClassesCreatedBetween(DateTime start, DateTime end);
        Task<List<ClassTn>> GetActiveClassesAsync();
        Task<List<ClassTn>> GetInactiveClassesAsync();
        Task<List<Users>> GetStudentsInClassAsync(int classId);
        Task<List<Exam>> GetExamsOfClassAsync(int classId);
        Task<List<Notification>> GetNotificationsOfClassAsync(int classId);
        Task<int> CountAllAsync();
    }
}
