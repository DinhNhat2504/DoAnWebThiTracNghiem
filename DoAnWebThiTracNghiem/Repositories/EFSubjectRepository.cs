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
        public async Task<IEnumerable<Subject>> GetAllAsync()
        {
            
                return await _context.Subjects.ToListAsync();
            
            

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
        public async Task<int> CountAsync(int RoleAd, int id, string search)
        {
            if (RoleAd == 1)
            {
                return await _context.Subjects.Where(c => string.IsNullOrEmpty(search) || c.Subject_Name.Contains(search)).CountAsync();
            }
            else
            {
                return await _context.Subjects.Where(c => c.CreatorUser_Id == id && (string.IsNullOrEmpty(search) || c.CreatorUser_Id == id && c.Subject_Name.Contains(search))).CountAsync();
            }
        }
        public async Task<IEnumerable<Subject>> GetPagedAsync(int RoleAd, int id, int page, int pageSize, string search)
        {
            if (RoleAd == 1)
            {
                return await _context.Subjects
                    .Where(c => string.IsNullOrEmpty(search) || c.Subject_Name.Contains(search))
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Include(c => c.Creator)
                    .ToListAsync();
            }
            else
            {
                return await _context.Subjects
                    .Where(c => c.CreatorUser_Id == id && (string.IsNullOrEmpty(search) || c.CreatorUser_Id == id && c.Subject_Name.Contains(search)))
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Include(c => c.Creator)
                    .ToListAsync();
            }
        }
        public async Task<List<Subject>> GetSubjectsCreatedBetween(DateTime start, DateTime end)
        {
            return await _context.Subjects
                .Where(s => s.CreateAt >= start && s.CreateAt <= end)
                .ToListAsync();
        }
        public async Task<int> CountAllAsync()
        {
            return await _context.Subjects.CountAsync();
        }
    }

    }

