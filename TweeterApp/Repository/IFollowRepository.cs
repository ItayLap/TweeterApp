using TweeterApp.Models;

namespace TweeterApp.Repository
{
    public interface IFollowRepository
    {
        Task<bool> IsFollowingAsync(int followerId, int followeeId);
        Task<IEnumerable<ApplicationUser>> GetFollowersAsync(int userId);
        Task<IEnumerable<ApplicationUser>> GetFollowingAsync(int userId);
        Task AddAsync(int followerId, int followeeId);
        Task RemoveAsync(int followerId,int followeeId);
    }
}
