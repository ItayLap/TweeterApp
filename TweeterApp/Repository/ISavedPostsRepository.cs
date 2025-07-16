using TweeterApp.Models;

namespace TweeterApp.Repository
{
    public interface ISavedPostsRepository
    {
        Task SavePostAsync(int postId, int userId);
        Task RemoveSavedPostAsync(int postId, int userId);
        Task<bool> IsSavedAsync (int postId, int userId);
        Task<List<PostModel>> GetSavedPostsAsync (int userId);
    }
}
