using TweeterApp.Models;

namespace TweeterApp.Repository
{
    public interface IPostRepository
    {
        Task<PostModel> GetByIdAsync(int id);
        Task<IEnumerable<PostModel>> GetAllAsync();
        Task AddAsync(PostModel post);
        Task UpdateAsync(PostModel post);
        Task DeleteAsync(int id);
    }
}
