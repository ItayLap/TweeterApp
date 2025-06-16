using Microsoft.EntityFrameworkCore;
using TweeterApp.Data;
using TweeterApp.Models;

namespace TweeterApp.Repository
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDbContext _context;
        public CommentRepository(ApplicationDbContext context) 
        {
            _context = context;
        }

        public async Task AddAsync(CommentModel comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<CommentModel> GetByIdAsync(int id)
        {
            return await _context.Comments.FindAsync(id);
        }

        public async Task<IEnumerable<CommentModel>> GetByPostIdAsync(int postId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Where(c=>c.PostId == postId)
                .OrderByDescending(c=>c.CreatedDate)
                .ToListAsync();
        }

        public async Task UpdateAsync(CommentModel comment)
        {
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetLikeCountAsync(int commentId)
        {
            return await _context.CommentLikes.CountAsync(I => I.CommentId == commentId);
        }

        public async Task<bool> IsLikedByCurrentUser(int commentId, int userId)
        {
            return await _context.CommentLikes.AnyAsync(I => I.CommentId==commentId && I.UserId == userId);
        }

        public async Task<bool> ToggleLikeAsync(int commentId, int userId)
        {
            var existingLike = await _context.CommentLikes
                 .FirstOrDefaultAsync(cl => cl.CommentId == commentId && cl.UserId == userId);

            if (existingLike != null)
            {
                _context.CommentLikes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return false;
            }

            var like = new CommentLikeModel
            {
                CommentId = commentId,
                UserId = userId
            };

            _context.CommentLikes.Add(like);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
