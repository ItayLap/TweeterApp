using Microsoft.EntityFrameworkCore;
using TweeterApp.Data;
using TweeterApp.Models;

namespace TweeterApp.Repository
{
    public class LikeRepository : ILikeRepository
    {
        public readonly ApplicationDbContext _context;
        public LikeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddLikeAsync(LikeModel like)
        {
            _context.Likes.Add(like);
            await _context.SaveChangesAsync();
        }
        public async Task RemoveLikeAsync(LikeModel like)
        {
            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();
        }

        public async Task<LikeModel> GetLikeAsync(int UserId, int PostId)
        {
            return await (_context.Likes.FirstOrDefaultAsync(I => I.UserId == UserId && I.PostId == PostId));
        }

        public async Task<int> GetLikeCountAsync(int PostId)
        {
            return await _context.Likes.CountAsync(i => i.PostId == PostId); 
        }

        public async Task<bool> IsLikedAsync(int userId, int PostId)
        {
            return await (_context.Likes.AnyAsync(I => I.UserId == userId && I.PostId == PostId));  
        }
    }
}
