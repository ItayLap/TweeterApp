using Microsoft.EntityFrameworkCore;
using TweeterApp.Data;
using TweeterApp.Models;

namespace TweeterApp.Repository
{
    public class FollowRepository : IFollowRepository
    {
        private readonly ApplicationDbContext _context;
        public FollowRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(int followerId, int followeeId)
        {
            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);

            if (follow != null)
            {
                _context.Follows.Remove(follow);
                await _context.SaveChangesAsync();
            }
        }
        public async Task RemoveAsync(int followerId, int followeeId)
        {
            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);

            if (follow != null)
            {              
                _context.Follows.Remove(follow);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ApplicationUser>> GetFollowersAsync(int userId)
        {
            return await _context.Follows
                .Where(f => f.FolloweeId == userId)
                .Select(f => f.Follower)
                .ToListAsync();
        }

        public async Task<IEnumerable<ApplicationUser>> GetFollowingAsync(int userId)
        {
            return await _context.Follows
             .Where(f => f.FollowerId == userId)
             .Select(f => f.Followee)
             .ToListAsync();
        }

        public async Task<bool> IsFollowingAsync(int followerId, int followeeId)
        {
            return await (_context.Follows.AnyAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId));
        }
    }
}
