using DoAnWebThiTracNghiem.Areas.Admin.Models;
using DoAnWebThiTracNghiem.Models;

namespace DoAnWebThiTracNghiem.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<Users>> GetAllAsync();
        Task<IEnumerable<Roles>> GetAllRoleAsync();
        Task<Users> GetByIdAsync(int id);
        Task AddAsync(Users user);
        Task UpdateAsync(Users user);
        Task DeleteAsync(int id);
        Task<IEnumerable<Users>> GetPagedAsync(int page, int pageSize , string search);
        Task<int> CountAsync(string search);
        Task<List<Users>> GetUsersCreatedBetween(DateTime start, DateTime end);
        Task<List<UserActivityCountViewModel>> GetUserActivityCountsAsync(List<int> userIds, List<int> roleIds);
        Task<bool> ExistsByEmailAsync(string email);
        Task<int> CountByRoleAsync(string role);
        Task<List<Users>> GetRecentUsersAsync(int take);

    }
}
