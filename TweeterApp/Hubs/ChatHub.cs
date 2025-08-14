using Microsoft.AspNetCore.SignalR;

namespace TweeterApp.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly Dictionary<string, string> _conections = new();
        public override Task OnConnectedAsync()
        {
            var username = Context.User?.Identity?.Name;
            if(!string.IsNullOrEmpty(username))
            {
                _conections[username] = Context.ConnectionId;
            }
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception exeption)
        {
            var username = Context.User?.Identity.Name;
            if (!string.IsNullOrEmpty(username))
            {
                _conections.Remove(username);
            }
            return base.OnDisconnectedAsync(exeption);
        }
        public async Task SendMessage(string toEmail,string message)
        {
            var fromUsername = Context.User?.Identity.Name ?? "Anonymous";
            if (_conections.TryGetValue(toEmail, out var connectionId))          
            {
                await Clients.Client(connectionId).SendAsync("RecieveMessage", fromUsername, message);
            }
        }

    }
}
