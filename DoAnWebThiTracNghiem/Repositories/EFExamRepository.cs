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
            return await _context.Exams
                .Include(e => e.Subject)
                .Include(e => e.Creator)
                .Where(e => e.CreatorUser_Id == id)
                .ToListAsync();
        }
        public async Task<Exam> GetByIdAsync(int id)
        {
            return await _context.Exams
                .Include(e => e.Subject)
                .Include(e => e.Creator)
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
    }
}
