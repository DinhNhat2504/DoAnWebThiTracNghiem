using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnWebThiTracNghiem.Repositories
{
    public class EFExamRepository : IExamRepository
    {
        private readonly AppDBContext _context;
        public EFExamRepository(AppDBContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Exam>> GetAllAsync(int id)
        {
            if (id == 1)
            {
                return await _context.Exams
                    .Include(e => e.Subject)
                    .Include(e => e.Creator)
                    .ToListAsync();
            }
            else
            {
                return await _context.Exams
                    .Include(e => e.Subject)
                    .Include(e => e.Creator)
                    .Where(e => e.CreatorUser_Id == id)
                    .ToListAsync();
            }

        }
        public async Task<Exam> GetByIdAsync(int id)
        {
            return await _context.Exams
                .Include(e => e.Subject)
                .Include(e => e.Creator)
                .FirstOrDefaultAsync(e => e.Exam_ID == id);
        }
        public async Task<Exam> GetByIdQSAsync(int id)
        {
            return await _context.Exams
                .Include(e => e.Subject)
                .Include(e => e.Creator)
                .Include(e => e.Exam_Questions)
                    .ThenInclude(eq => eq.Question)
                .FirstOrDefaultAsync(e => e.Exam_ID == id);
        }
        public async Task AddAsync(Exam exam)
        {
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Exam exam)
        {
            _context.Exams.Update(exam);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var exam = await _context.Exams.FindAsync(id);

            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync();

        }
        public async Task<IEnumerable<Exam>> GetPagedAsync(int RoleAd, int id, int page, int pageSize, string search)
        {
            if (RoleAd == 1)
            {
                return await _context.Exams
                    .Where(e => string.IsNullOrEmpty(search) || e.Exam_Name.Contains(search) || e.Subject.Subject_Name.Contains(search))
                    .OrderBy(u => u.Exam_ID)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Include(u => u.Subject)
                    .Include(u => u.Creator)
                    .ToListAsync();
            }
            else
            {
                return await _context.Exams
                    .Where(e => e.Creator.User_Id == id && (string.IsNullOrEmpty(search) || e.Exam_Name.Contains(search) || e.Subject.Subject_Name.Contains(search)))
                    .OrderBy(u => u.Exam_ID)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Include(u => u.Subject)
                    .Include(u => u.Creator)
                    .ToListAsync();
            }

        }
        public async Task<int> CountAsync(int roleId, int creatorId, string search)
        {
            if (roleId == 1)
            {
                return await _context.Exams.Where(e => string.IsNullOrEmpty(search) || e.Exam_Name.Contains(search) || e.Subject.Subject_Name.Contains(search)).CountAsync();
            }
            else
            {
                return await _context.Exams.CountAsync(e => e.CreatorUser_Id == creatorId && (string.IsNullOrEmpty(search) || e.Exam_Name.Contains(search) || e.Subject.Subject_Name.Contains(search)));
            }

        }
        public async Task<List<Exam>> GetExamsCreatedBetween(DateTime start, DateTime end)
        {
            return await _context.Exams
                .Where(c => c.CreateAt >= start && c.CreateAt <= end)
                .ToListAsync();
        }
        public async Task<int> CountAllAsync()
        {
            return await _context.Exams.CountAsync();
        }
    }
}
