using Microsoft.AspNetCore.Mvc;

namespace TweeterApp.Controllers
{
    public class NotificationsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
