using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TweeterApp.Models;
using TweeterApp.Repository;

namespace TweeterApp.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserNotificationRepository _notificationRepository;

        public NotificationsController(UserManager<ApplicationUser> userManager, IUserNotificationRepository notificationRepository)
        {
            _userManager = userManager;
            _notificationRepository = notificationRepository;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var notifications = await _notificationRepository.GetUserNotificationsAsync(user.Id);
            return View(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationRepository.MarkAsReadAsync(id);
            return RedirectToAction("Index");
        }
    }
}
