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
        public async Task<IEnumerable<ClassTn>> GetAllAsync(int RoleAd, int id)
        {
            if (RoleAd == 1)
            {
                return await _context.ClassTn.Include(c => c.Subject).ToListAsync();
            }
            else
            {
                return await _context.ClassTn.Where(c => c.CreatorUser_Id == id)
                .Include(c => c.Subject)
                .ToListAsync();
            }
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
        public async Task<int> CountAsync(int RoleAd,int id , string search)
        {
            if (RoleAd == 1)
            {
                return await _context.ClassTn
                    .Where(c => string.IsNullOrEmpty(search) || c.ClassName.Contains(search))
                    .CountAsync();
            }
            else
            {
                return await _context.ClassTn
                    .Where(c => c.CreatorUser_Id == id && (string.IsNullOrEmpty(search) || c.ClassName.Contains(search)))
                    .CountAsync();
            }
            
        }

        public async Task<IEnumerable<ClassTn>> GetPagedAsync(int RoleAd, int id, int page, int pageSize, string search)
        {
            if (RoleAd == 1)
            {
                return await _context.ClassTn
                    .Where(c => string.IsNullOrEmpty(search) || c.ClassName.Contains(search))
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Include(c => c.Subject)
                    .Include(c => c.Creator)
                    .ToListAsync();
            }
            else
            {
                return await _context.ClassTn
                    .Where(c => c.CreatorUser_Id == id && (string.IsNullOrEmpty(search) || c.ClassName.Contains(search)))
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Include(c => c.Subject)
                    .Include(c => c.Creator)
                    .ToListAsync();
            }
        }

        public async Task<List<Subject>> GetAllSubjectsAsync(int RoleAd, int id)
        {
            
            if (RoleAd == 1)
            {
                return await _context.Subjects.ToListAsync();
            }
            else
            {
                return await _context.Subjects.Where(s => s.CreatorUser_Id == id).ToListAsync();
            }  
        }

        public async Task<List<Subject>> GetSubjectsByCreatorAsync(int creatorUserId)
        {
            return await _context.Subjects
                .Where(s => s.CreatorUser_Id == creatorUserId)
                .ToListAsync();
           
        }

    }

}
