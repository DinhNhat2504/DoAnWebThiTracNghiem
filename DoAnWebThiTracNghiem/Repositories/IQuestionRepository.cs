using DoAnWebThiTracNghiem.Models;

namespace DoAnWebThiTracNghiem.Repositories
{
    public interface IQuestionRepository
    {
        Task<IEnumerable<Question>> GetAllAsync(int examId);
        Task<Question> GetByIdAsync(int id);
        Task AddAsync(Question question);
        Task UpdateAsync(Question question);
        Task DeleteAsync(int id);
    }
}
