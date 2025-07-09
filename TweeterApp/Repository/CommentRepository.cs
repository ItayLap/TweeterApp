using Microsoft.AspNetCore.Mvc;
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
                .Where(c=>c.PostId == postId)
                .Include(c => c.User)
                .Include(c=>c.Likes)
                .ToListAsync();
        }

        public async Task UpdateAsync(CommentModel comment)
        {
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
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

            var comment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment != null && comment.UserId != userId)
            {
                var sender = await _context.Users.FindAsync(userId);

                var notification = new NotificationModel
                {
                    RecipiantId = comment.UserId,
                    SenderId = userId,
                    Message = $"{sender.UserName} liked your comment:\"{comment.Content}\"",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                };
                _context.Notifications.Add(notification);

            }

            _context.CommentLikes.Add(like);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CommentModel>> GetCommentsForPostAsync(int postId, int? currentUserId = null)
        {
            var comments =await _context.Comments
                .Where(c => c.PostId==postId)
                .Include(c=>c.User)
                .Include(c=>c.Likes)
                .ToListAsync();

            foreach (var comment in comments)
            {
                comment.IsLikedByCurrentUser = comment.Likes.Any(I => I.UserId == currentUserId);
            }

            return comments;
        }
    }
}
