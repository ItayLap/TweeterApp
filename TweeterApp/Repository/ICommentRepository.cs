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

        Task<bool> ToggleLikeAsync(int commentId, int userId);
        Task<int> GetLikeCountAsync(int commentId);
        Task<bool> IsLikedByCurrentUser(int commentId,int userId);
    }
}
