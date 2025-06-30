using Microsoft.EntityFrameworkCore;
using TweeterApp.Data;
using TweeterApp.Models;

namespace TweeterApp.Repository
{
    public class UserNotificationRepository : IUserNotificationRepository
    {
        private readonly ApplicationDbContext _context;
        public UserNotificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddNotificationAsync(NotificationModel notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<NotificationModel>> GetUserNotificationsAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.RecipiantId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notif = await _context.Notifications.FindAsync(notificationId);
            if (notif != null)
            {
                notif.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
