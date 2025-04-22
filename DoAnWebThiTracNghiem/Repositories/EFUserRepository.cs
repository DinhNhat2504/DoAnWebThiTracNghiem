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
            return await _context.Users.Include(u=> u.Role).ToListAsync();
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
    }
}
