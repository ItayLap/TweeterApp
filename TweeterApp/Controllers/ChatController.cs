using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
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
        public async Task<IActionResult> Index()
        {
            var me = User?.Identity?.Name ?? "";
            var friends = await _context.Friends
                .Where(f => f.Status == FriendshipStatus.Accepted && (f.RequesterUserName == me || f.AddresseeUserName == me))
                .Select(f => f.RequesterUserName == me ? f.AddresseeUserName : f.RequesterUserName)
                .OrderBy(n => n)
                .ToListAsync();
            var vm = new StartChatViewModel
            {
                Users = friends,
            };
            return View(vm);
        }

        [HttpGet("/chat/with/{**userName}")]
        public IActionResult With(string userName)
        {
            if(string.IsNullOrEmpty(userName))return NotFound();
            var me = User.Identity!.Name!.ToLowerInvariant();
            var other = userName.ToLowerInvariant();

            var history = _context.Messages
                .Where(m =>
                (m.FromUser == me && m.ToUser == other) ||
                (m.FromUser == other && m.ToUser == me))
                .OrderBy(m => m.SentAt)
                .ToList();

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
