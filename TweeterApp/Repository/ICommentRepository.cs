using TweeterApp.Models;

namespace TweeterApp.Repository
{
    public interface ICommentRepository
    {
        Task<IEnumerable<CommentModel>> GetByPostIdAsync(int postId);
        Task<CommentModel> GetByIdAsync(int id);
        Task AddAsync(CommentModel comment);
        Task UpdateAsync(CommentModel comment);
        Task DeleteAsync(int id);
    }
}
