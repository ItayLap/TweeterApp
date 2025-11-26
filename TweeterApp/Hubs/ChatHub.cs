using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Reflection;
using TweeterApp.Data;
using TweeterApp.Models;

namespace TweeterApp.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _connections = new();
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _reactions = new();
        private static readonly ConcurrentDictionary<string, string> _userReactionByMessage = new();

        private readonly ApplicationDbContext _db;
        public ChatHub(ApplicationDbContext db) => _db = db;

        private static string ReactionKey(string messageId, string emoji) => $"{messageId}::{emoji}";
        private static string UserMsgKey(string messageId, string user) => $"{messageId}::{user}";


        public override Task OnConnectedAsync()
        {
            var username = Context.User?.Identity?.Name;
            if(username != null)
            {
                _connections.GetOrAdd(username, _ => new())[Context.ConnectionId] = 1;
                return base.OnConnectedAsync();
            }
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception exeption)
        {
            var username = Context.User.Identity.Name;
            if (username != null)
            {
                _connections.TryRemove(username, out _);
            }
            return base.OnDisconnectedAsync(exeption);
        }
        private static string DialougGroup(string UserA, string UserB)
        {
            var a = UserA.Trim().ToLowerInvariant();
            var b = UserB.Trim().ToLowerInvariant();
            return string.CompareOrdinal(a, b) <= 0 ? $"{a}|{b}" : $"{b}|{a}";
        }
        public async Task SendMessage(string toUsername,string message)
        {
            var fromUsername = Context.User.Identity.Name ?? "Anonymous";
            var sender = await _db.Users.FirstOrDefaultAsync(u => u.UserName == fromUsername);
            var receiver = await _db.Users.FirstOrDefaultAsync(u => u.UserName == toUsername);
            if (sender == null || receiver == null) return;

            if (string.IsNullOrWhiteSpace(toUsername) || string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(fromUsername)) return;
            if (!await AreFriends(toUsername, fromUsername)) return;
            var timestamp = DateTime.UtcNow;

            var msg = new MessageModel
            {
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                Body = message,
                SentAt = timestamp,
                IsRead = false,
                Subject = ""
            };
            _db.Messages.Add(msg);
            await _db.SaveChangesAsync();

            var group = DialougGroup(fromUsername, toUsername);
            var id = Guid.NewGuid().ToString("N");
            await Clients.Group(group).SendAsync("ReceiveMessage",msg.Id, fromUsername, message, msg.SentAt);
        }
        public async Task JoinDialoug(string otherUsername)
        {
            var me = Context.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(me) || string.IsNullOrWhiteSpace(otherUsername)) return;
            var group = DialougGroup(me, otherUsername);
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
            await Clients.Group(group).SendAsync("PresenceChanged", me, "online");
        }
        public async Task LeaveDialouge(string otherUsername)
        {
            var me = Context.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(me) || string.IsNullOrWhiteSpace(otherUsername)) return;
            var group = DialougGroup(me, otherUsername);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
            await Clients.Group(group).SendAsync("PresenceChanged", me, "offline");
        }
        public async Task ToggleReaction(string toUsername, string messageId, string emoji)
        {
            var me = Context.User?.Identity?.Name?.Trim();
            if (string.IsNullOrWhiteSpace(me) || string.IsNullOrWhiteSpace(toUsername) ||string.IsNullOrWhiteSpace(messageId)|| string.IsNullOrWhiteSpace(emoji)) return;
            var group =  DialougGroup(me, toUsername);
            var muKey =  UserMsgKey(messageId, me);
            if (_userReactionByMessage.TryGetValue(muKey, out var existingEmoji))
            {
                if (existingEmoji == emoji)
                {
                    var users = _reactions.GetOrAdd(ReactionKey(messageId, emoji),_ => new ConcurrentDictionary<string, byte>());
                    users.TryRemove(me, out _);
                    _userReactionByMessage.TryRemove(muKey, out _);

                    var count = users.Count;
                    if (count == 0) _reactions.TryRemove(ReactionKey(messageId, emoji), out _);

                    await Clients.Group(group).SendAsync("ReactionUpdated", new { messageId, emoji, count, user = me, added = false});
                }
                else
                {
                    var oldKey =ReactionKey(messageId, existingEmoji);
                    if(_reactions.TryGetValue(oldKey, out var oldUsers))
                    {
                        oldUsers.TryRemove(me, out _);
                        var oldCount = oldUsers.Count;
                        if(oldCount == 0) _reactions.TryRemove(oldKey, out _);

                        await Clients.Group(group).SendAsync("ReactionUpdated", new {messageId, emoji = existingEmoji, count = oldCount, user = me, added = false });
                    }
                    var newKey = ReactionKey(messageId, emoji);
                    var newUsers = _reactions.GetOrAdd(ReactionKey(messageId, emoji), _ => new ConcurrentDictionary<string, byte>());
                    newUsers[me] = 1;
                    _userReactionByMessage[muKey] = emoji;
                    var newCount = newUsers.Count;

                    await Clients.Group(group).SendAsync("ReactionUpdated", new {messageId, emoji, count = newCount, user = me, added = true });
                }

            }
            else
            {
                var users = _reactions.GetOrAdd(ReactionKey(messageId, emoji), _ => new ConcurrentDictionary<string, byte>());
                users[me] = 1;
                _userReactionByMessage[muKey] = emoji;
                var count = users.Count;

                await Clients.Group(group).SendAsync("ReactionUpdated", new { messageId, emoji, count, user = me, added = true });
            }
        }
        public async Task SendImage(string toUsername, string imageUrl, string? caption)
        {
            var from = Context.User?.Identity?.Name ?? "Anonymous";

            var sender = await _db.Users.FirstOrDefaultAsync(u => u.UserName == from);
            var receiver = await _db.Users.FirstOrDefaultAsync(u => u.UserName == toUsername);
            if (sender == null || receiver == null) return;

            if(string.IsNullOrWhiteSpace(toUsername)|| string.IsNullOrWhiteSpace(imageUrl)) return;
            //if (!await AreFriends(toUsername, )) return;
            var group = DialougGroup(from, toUsername);

            var msg = new MessageModel
            {
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                Caption = caption,
                Subject = "image",
                Type = MessageType.Image,
                ImageUrl = imageUrl,
            };
            _db.Messages.Add(msg);
            await _db.SaveChangesAsync();


            await Clients.Group(group).SendAsync("ReceiveImage", msg.Id, from, msg.ImageUrl, msg.Caption, .SentAt);
        }
        
        private Task<bool> AreFriends(string a, string b) =>
            _db.Friends.AnyAsync(f => f.Status == FriendshipStatus.Accepted && 
            ((f.RequesterUserName == a && f.AddresseeUserName == b) ||
            (f.RequesterUserName == b && f.AddresseeUserName == a)));
    }
}
