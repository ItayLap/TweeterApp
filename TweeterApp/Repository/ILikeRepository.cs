using TweeterApp.Models;

namespace TweeterApp.Repository
{
    public interface ILikeRepository
    {
        Task<bool> IsLikedAsync(int userId, int PostId);
        Task<int> GetLikeCountAsync(int PostId);
        Task AddLikeAsync(LikeModel like);
        Task RemoveLikeAsync(LikeModel like);
        Task<LikeModel> GetLikeAsync(int UserId, int PostId);
    }
}
