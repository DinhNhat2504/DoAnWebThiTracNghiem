using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnWebThiTracNghiem.Repositories
{
    public class EFQuestionRepository : IQuestionRepository
    {

        private readonly AppDBContext _context;
        public EFQuestionRepository(AppDBContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Question>> GetAllAsync(int Id)
        {
            if(Id == 1)
            {
                return await _context.Question
                    .Include(q => q.Subject)
                    .Include(q => q.Level)
                    .Include(q => q.Creator)
                    .ToListAsync();
            }
            else {
                return await _context.Question
                    .Include(q => q.Subject)
                    .Include(q => q.Level)
                    .Include(q => q.Creator)
                    .Where(q => q.CreatorUser_Id == Id)
                    .ToListAsync(); 
            }
                
        } 
        public async Task<Question> GetByIdAsync(int id)
        {
            return await _context.Question
                .Include(q => q.Subject)
                .Include(q => q.Level)
                .Include(q => q.Creator)
                .FirstOrDefaultAsync(q => q.Question_ID == id);
        }
        public async Task AddAsync(Question question)
        {
            _context.Question.Add(question);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Question question)
        {
            _context.Question.Update(question);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var question = await _context.Question.FindAsync(id);
            if (question != null)
            {
                _context.Question.Remove(question);
                await _context.SaveChangesAsync();
            }
        }
    }
}
