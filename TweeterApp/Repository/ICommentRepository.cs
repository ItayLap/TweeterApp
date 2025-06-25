using TweeterApp.Models;

namespace TweeterApp.Repository
{
    public interface ICommentRepository
    {
        Task<IEnumerable<CommentModel>> GetByPostIdAsync(int postId);
        Task<IEnumerable<CommentModel>> GetCommentsForPostAsync(int postId, int? currentUserId = null);
        Task<CommentModel> GetByIdAsync(int id);
        Task AddAsync(CommentModel comment);
        Task UpdateAsync(CommentModel comment);
        Task DeleteAsync(int id);

        Task<bool> ToggleLikeAsync(int commentId, int userId);
    }
}
