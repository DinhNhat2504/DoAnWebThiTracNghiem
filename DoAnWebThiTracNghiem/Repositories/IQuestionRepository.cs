using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.ViewModel;

namespace DoAnWebThiTracNghiem.Repositories
{
    public interface IQuestionRepository
    {
        Task<IEnumerable<Question>> GetAllAsync(int examId);
        Task<Question> GetByIdAsync(int id);
        Task AddAsync(Question question);
        Task UpdateAsync(Question question);
        Task DeleteAsync(int id);
        Task<int> CountAsync(int roleId, int userId, string search, int? subjectId, int? levelId, int? questionTypeId);
        Task<IEnumerable<Question>> GetPagedAsync(int roleId, int userId, int page, int pageSize, string search, int? subjectId, int? levelId, int? questionTypeId);

        Task<List<SubjectViewModel>> GetAllSubjectsAsync(int RoleAd, int id);
        Task<List<Subject>> GetSubjectsByCreatorAsync( int id);
        Task<List<QuestionType>> GetAllQuestionTypesAsync();
        Task<List<Level>> GetAllLevelsAsync();
        Task<List<Question>> GetQuestionsCreatedBetween(DateTime start, DateTime end);
        Task<string> GetSubjectNameByIdAsync(int id);
        Task<string> GetLevelNameByIdAsync(int id);
        Task<string> GetQTypeNameByIdAsync(int id);
        Task<int> CountAllAsync();
    }
}
