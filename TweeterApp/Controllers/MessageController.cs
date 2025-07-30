using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using TweeterApp.Data;
using TweeterApp.Models;

namespace TweeterApp.Controllers
{
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessageController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Inbox()
        {
            var user = _userManager.GetUserAsync(User);
            var userId = user.Id;
            var messages = await _context.Messages
                .Where(m=> m.ReceiverId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
            return View(messages);
        }
        public async Task<IActionResult> Outbox()
        {
            var user = _userManager.GetUserAsync(User);
            var userId = user.Id;
            var messages = await _context.Messages
                .Where(m => m.SenderId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
            return View(messages);
        }
        public IActionResult Send()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Send(string reciverEmail, string subject, string body)
        {
            var sender = await _userManager.GetUserAsync(User);
            if (string.IsNullOrEmpty(reciverEmail))
            {
                ModelState.AddModelError("", "Receiver email is required");
                return View();
            }

            var reciever = await _userManager.FindByEmailAsync(reciverEmail);
            if (reciever == null)
            {
                ModelState.AddModelError("", "User not found");
                return View();
            }
            var Message = new MessageModel
            {
                SenderId = sender.Id,
                ReceiverId = reciever.Id,
                Subject = subject,
                Body = body,
                SentAt = DateTime.UtcNow
            };
            _context.Messages.Add(Message);
            await _context.SaveChangesAsync();
            return RedirectToAction("Outbox");
        }
    }
}
