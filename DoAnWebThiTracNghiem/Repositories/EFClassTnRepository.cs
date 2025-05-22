using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.ViewModel;
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
        public async Task<int> CountAsync(int RoleAd, int id, string search)
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

        public async Task<bool> Check(string code)
        {
            return await _context.ClassTn.AnyAsync(c => c.InviteCode == code);
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

        public async Task<List<SubjectViewModel>> GetAllSubjectsAsync(int RoleAd, int id)
        {
            IQueryable<Subject> query = _context.Subjects.Include(s => s.Creator);

            if (RoleAd != 1)
            {
                query = query.Where(s => s.CreatorUser_Id == id);
            }

            return await query
                .GroupBy(s => new { s.Subject_Name, s.CreatorUser_Id })
                .Select(g => new SubjectViewModel
                {
                    Subject_Id = g.First().Subject_Id,
                    Subject_Name = g.First().Subject_Name,
                    CreatorName = g.First().Creator.FullName
                })
                .ToListAsync();
        }

        public async Task<List<Subject>> GetSubjectsByCreatorAsync(int creatorUserId)
        {
            return await _context.Subjects
                .Where(s => s.CreatorUser_Id == creatorUserId)
                .ToListAsync();

        }
        public async Task<List<ClassTn>> GetClassesCreatedBetween(DateTime start, DateTime end)
        {
            return await _context.ClassTn
                .Where(c => c.CreatedAt >= start && c.CreatedAt <= end)
                .ToListAsync();
        }

        public async Task<List<ClassTn>> GetActiveClassesAsync()
        {
            return await _context.ClassTn
                .Where(c => _context.ClassStudents.Any(sc => sc.Class_ID == c.Class_Id))
                .ToListAsync();
        }

        public async Task<List<ClassTn>> GetInactiveClassesAsync()
        {
            return await _context.ClassTn
                .Where(c => !_context.ClassStudents.Any(sc => sc.Class_ID == c.Class_Id))
                .ToListAsync();
        }
        public async Task<List<Users>> GetStudentsInClassAsync(int classId)
        {
            return await _context.ClassStudents
                .Where(sc => sc.Class_ID == classId)
                .Include(sc => sc.User)
                .Select(sc => sc.User)
                .ToListAsync();
        }

        public async Task<List<Exam>> GetExamsOfClassAsync(int classId)
        {
            return await _context.ClassExams
                .Where(ec => ec.ClassTNClass_Id == classId)
                .Include(ec => ec.Exam)
                .Select(ec => ec.Exam)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetNotificationsOfClassAsync(int classId)
        {
            // Giả sử Notification có thuộc tính ClassId
            return await _context.Notifications
                .Where(n => n.ClassTNClass_Id == classId)
                .OrderByDescending(n => n.Timestamp)
                .ToListAsync();
        }
        public async Task<int> CountAllAsync()
        {
            return await _context.ClassTn.CountAsync();
        }
    }

}
