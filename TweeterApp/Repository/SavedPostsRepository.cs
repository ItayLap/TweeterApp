using Microsoft.EntityFrameworkCore;
using TweeterApp.Data;
using TweeterApp.Models;

namespace TweeterApp.Repository
{
    public class SavedPostsRepository : ISavedPostsRepository
    {
        private readonly ApplicationDbContext _context;
        public SavedPostsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PostModel>> GetSavedPostsAsync(int userId)
        {
            return await _context.SavedPosts
                .Where(s => s.UserId == userId)
                .Include(s => s.Post).ThenInclude(p => p.User)
                .Select(s => s.Post)
                .ToListAsync();
        }

        public async Task<bool> IsSavedAsync(int postId, int userId)
        {
            return await _context.SavedPosts.AnyAsync(s => s.PostId == postId && s.UserId == userId);
        }

        public async Task RemoveSavedPostAsync(int postId, int userId)
        {
            var saved = await _context.SavedPosts.FirstOrDefaultAsync(s => s.PostId == postId && s.UserId == userId);
            if (saved != null)
            {
                _context.SavedPosts.Remove(saved);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SavePostAsync(int postId, int userId)
        {
            if (!await _context.SavedPosts.AnyAsync(s => s.PostId == postId && s.UserId == userId))
            {
                _context.SavedPosts.Add(new SavedPostsModel { PostId = postId, UserId = userId });
                await _context.SaveChangesAsync();
            }
        }
    }
}
