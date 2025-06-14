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
    }
}
