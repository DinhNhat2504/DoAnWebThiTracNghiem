using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnWebThiTracNghiem.Repositories
{
    public class EFSubjectRepository : ISubjectRepository
    {
        private readonly AppDBContext _context;
        public EFSubjectRepository(AppDBContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Subject>> GetAllAsync(int id)
        {
            if(id == 0)
            {
                return await _context.Subjects.ToListAsync();
            }
            else {
                return await _context.Subjects.Where(c => c.CreatorUser_Id == id).ToListAsync(); 
            }
                
        }
        public async Task<Subject> GetByIdAsync(int id)
        {

            return await _context.Subjects.Include(s => s.Creator).FirstOrDefaultAsync(s => s.Subject_Id == id);
        }
        public async Task AddAsync(Subject subject)
        {
           _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Subject subject)
        {
            _context.Subjects.Update(subject);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject != null)
            {
                _context.Subjects.Remove(subject);
                await _context.SaveChangesAsync();
            }
        }
    }
    

    }

