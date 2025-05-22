using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.ViewModel;
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
                .Include(q => q.QuestionType)
                .FirstOrDefaultAsync(q => q.Question_ID == id);
        }
        public async Task<string> GetSubjectNameByIdAsync(int id)
        {
            return await _context.Subjects
                .Where(s => s.Subject_Id == id)
                .Select(s => s.Subject_Name)
                .FirstOrDefaultAsync();
        }
        public async Task<string> GetLevelNameByIdAsync(int id)
        {
            return await _context.Levels
                    .Where(l => l.Id == id)
                    .Select(l => l.LevelName)
                    .FirstOrDefaultAsync();
        }

        public async Task<string> GetQTypeNameByIdAsync(int id)
        {
            return await _context.QuestionType
                    .Where(qt => qt.Id == id)
                    .Select(qt => qt.Name)
                    .FirstOrDefaultAsync();
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
        public async Task<int> CountAsync(int roleAd, int userId, string search, int? subjectId, int? levelId, int? questionTypeId)
        {
            var query = _context.Question.AsQueryable();

            if (roleAd != 1)
                query = query.Where(q => q.CreatorUser_Id == userId);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(q => q.Question_Content.Contains(search));

            if (subjectId.HasValue)
                query = query.Where(q => q.Subject_ID == subjectId.Value);

            if (levelId.HasValue)
                query = query.Where(q => q.Level_ID == levelId.Value);

            if (questionTypeId.HasValue)
                query = query.Where(q => q.QuestionTypeId == questionTypeId.Value);

            return await query.CountAsync();
        }

        public async Task<IEnumerable<Question>> GetPagedAsync(int roleAd, int userId, int page, int pageSize, string search, int? subjectId, int? levelId, int? questionTypeId)
        {
            var query = _context.Question
                .Include(q => q.Subject)
                .Include(q => q.Creator)
                .Include(q => q.QuestionType)
                .Include(q => q.Level)
                .AsQueryable();

            if (roleAd != 1)
                query = query.Where(q => q.CreatorUser_Id == userId);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(q => q.Question_Content.Contains(search));

            if (subjectId.HasValue)
                query = query.Where(q => q.Subject_ID == subjectId.Value);

            if (levelId.HasValue)
                query = query.Where(q => q.Level_ID == levelId.Value);

            if (questionTypeId.HasValue)
                query = query.Where(q => q.QuestionTypeId == questionTypeId.Value);

            return await query
                .OrderByDescending(q => q.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
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

        public async Task<List<QuestionType>> GetAllQuestionTypesAsync()
        {
           
                return await _context.QuestionType.ToListAsync();
            
           
        }

        public async Task<List<Level>> GetAllLevelsAsync()
        {
            
                return await _context.Levels.ToListAsync(); 
            
           
        }
        public async Task<List<Question>> GetQuestionsCreatedBetween(DateTime start, DateTime end)
        {
            return await _context.Question
                .Where(q => q.CreatedAt >= start && q.CreatedAt <= end)
                .ToListAsync();
        }
        public async Task<int> CountAllAsync()
        {
            return await _context.Question.CountAsync();
        }
    }
}
