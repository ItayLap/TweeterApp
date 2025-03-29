using Microsoft.EntityFrameworkCore;
using TweeterApp.Data;
using TweeterApp.Models;

namespace TweeterApp.Repository
{
    public class PostRepository : IPostRepository
    {
        private readonly ApplicationDbContext _context;
        public PostRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PostModel post)
        {
           _context.Posts.Add(post);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var Post = await _context.Posts.FindAsync(id);
            if (Post != null)
            {
                _context.Posts.Remove(Post);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<PostModel>> GetAllAsync()
        {
            return await _context.Posts.Include(p => p.User).ToListAsync();
        }

        public async Task<PostModel> GetByIdAsync(int id)
        {
            return await _context.Posts.Include(p => p.User).FirstOrDefaultAsync(p=>p.Id == id);
        }

        public async Task UpdateAsync(PostModel post)
        {
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
        }
    }
}
