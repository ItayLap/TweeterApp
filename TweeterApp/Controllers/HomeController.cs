using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TweeterApp.Data;
using TweeterApp.Migrations;
using TweeterApp.Models;

namespace TweeterApp.Controllers
{
    public class StartChatViewModel
    {
        public string? SelectedEmail { get; set; }
        public List<string> Users { get; set; } = new();
    }
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var me = User?.Identity?.Name ?? "";
            var meLower = me.ToLowerInvariant();
            var friends = _context.Friends
                .Where(f => f.Status == FriendshipStatus.Accepted &&
                                (f.RequesterUserName == meLower || f.AddresseeUserName == meLower))
                .Select(f => f.RequesterUserName == meLower ? f.AddresseeUserName : f.RequesterUserName)
                .OrderBy(f => f)
                .ToList();
            var vm = new StartChatViewModel { Users = friends };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartChat(StartChatViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.SelectedEmail))
            {
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction ("With", "Chat", new {username = model.SelectedEmail});
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
