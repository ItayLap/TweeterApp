using TweeterApp.Models;

namespace TweeterApp.Repository
{
    public interface IUserNotificationRepository
    {
        Task<IEnumerable<NotificationModel>> GetUserNotificationsAsync(int userId);
        Task AddNotificationAsync(NotificationModel notification);
        Task MarkAsReadAsync(int notificationId);
    }
}
