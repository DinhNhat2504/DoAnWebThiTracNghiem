using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnWebThiTracNghiem.Repositories
{
    public class EFClassTnRepository : IClassTnRepository
    {
        private readonly AppDBContext _context;
        public EFClassTnRepository(AppDBContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<ClassTn>> GetAllAsync(int id)
        {
            return await _context.ClassTn.Where(c => c.CreatorUser_Id == id)
                .Include(c => c.Subject)
                .ToListAsync();
        }
        public async Task<ClassTn> GetByIdAsync(int id)
        {
            return await _context.ClassTn.Include(s => s.Creator).FirstOrDefaultAsync(s => s.Class_Id == id);
        }
        public async Task AddAsync(ClassTn classTn)
        {
            _context.ClassTn.Add(classTn);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(ClassTn classTn)
        {
            _context.ClassTn.Update(classTn);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var classTn = await _context.ClassTn.FindAsync(id);

            _context.ClassTn.Remove(classTn);
            await _context.SaveChangesAsync();

        }

    }

}
