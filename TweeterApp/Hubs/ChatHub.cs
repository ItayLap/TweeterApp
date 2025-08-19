using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualBasic;
using System.Collections.Concurrent;

namespace TweeterApp.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _connections = new();
        public override Task OnConnectedAsync()
        {
            var username = Context.User?.Identity?.Name;
            if(!string.IsNullOrEmpty(username))
            {
                _connections[username] = Context.ConnectionId;
                return base.OnConnectedAsync();
            }
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception exeption)
        {
            var username = Context.User.Identity.Name;
            if (!string.IsNullOrEmpty(username))
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
        public async Task SendMessage(string toEmail,string message)
        {
            var fromUsername = Context.User.Identity.Name ?? "Anonymous";
            if (string.IsNullOrWhiteSpace(toEmail) || string.IsNullOrWhiteSpace(message)) return;
            var group = DialougGroup(fromUsername, toEmail);
            await Clients.Group(group).SendAsync("ReciveMessage", fromUsername, message, DateTimeOffset.UtcNow);
        }
        public async Task JoinDialouge(string otherUsername)
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
    }
}
