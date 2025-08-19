using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TweeterApp.Data;
using TweeterApp.Models;

namespace TweeterApp.Controllers
{
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/chat/with/{**userName}")]
        public IActionResult With(string userName)
        {
            if(string.IsNullOrEmpty(userName))return NotFound();
            ViewData["OtherUser"] = userName;
            return View("Chat");
        }
        //public async Task<IActionResult> Index()
        //{
        //    var messages = await _context.Messages
        //        .OrderBy(m => m.SentAt)
        //        .ToListAsync();
        //    return View(messages);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Send(string username, string text)
        //{
        //    if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(text))
        //    {
        //        var message = new MessageModel
        //        {
        //            Username = username,
        //            Text = text,
        //            SentAt = DateTime.Now,
        //        };
        //    }
        //}

    }
}
