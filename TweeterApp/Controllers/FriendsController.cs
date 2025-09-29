using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using TweeterApp.Data;
using TweeterApp.Hubs;
using TweeterApp.Models;

namespace TweeterApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("friends")]
    public class FriendsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHubContext<ChatHub> _hub;

        public FriendsController(ApplicationDbContext db, IHubContext<ChatHub> hub)
        {
            _db = db; 
            _hub = hub;
        }

        [HttpGet("/Friends/Manage")]
        public IActionResult Manage() => View();

        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var me = User.Identity!.Name!;
            var friends = await _db.Friends
                .Where(f => f.Status == FriendshipStatus.Accepted && f.RequesterUserName == me || f.AddresseeUserName == me)
                .Select(f =>  f.RequesterUserName == me ? f.AddresseeUserName: f.RequesterUserName).OrderBy(s => s).ToListAsync();

            return Ok(new { friends });
        }
        
        [HttpGet("request")]
        public async Task<IActionResult> Requests()
        {
            var me = User.Identity!.Name!;
            var incoming = await _db.Friends
                .Where(f => f.Status == FriendshipStatus.Pending && f.AddresseeUserName == me)
                .Select(f => f.RequesterUserName)
                .OrderBy(s => s)
                .ToListAsync();

            var outgoing = await _db.Friends
                .Where(f => f.Status == FriendshipStatus.Pending && f.RequesterUserName == me)
                .Select(f => f.AddresseeUserName)
                .OrderBy(s => s)
                .ToListAsync();
            return Ok(new {incoming, outgoing});
        }

        [HttpPost("request")]
        public async Task<IActionResult> SendRequest([FromBody] string toUserName)
        {
            var me = User.Identity!.Name!;
            toUserName = toUserName?.Trim().ToLowerInvariant() ?? "";
            if (string.IsNullOrWhiteSpace(toUserName) || toUserName == me) return BadRequest();
            var existing = await _db.Friends
                .FirstOrDefaultAsync(f =>
                (f.RequesterUserName == me && f.AddresseeUserName == toUserName) ||
                (f.RequesterUserName == toUserName && f.AddresseeUserName == me));
            if (existing != null)
            {
                switch (existing.Status)
                {
                    case FriendshipStatus.Accepted:
                        return Conflict("Already Friends");

                    case FriendshipStatus.Pending:
                        if (existing.RequesterUserName == toUserName && existing.AddresseeUserName == me)
                        {
                            existing.Status = FriendshipStatus.Accepted;
                            existing.UpdatedAt = DateTimeOffset.UtcNow;
                            await _db.SaveChangesAsync();
                            await _hub.Clients.User(toUserName).SendAsync("FriendRequestAccepted", me);
                            await _hub.Clients.User(me).SendAsync("FriendRequestAccepted", toUserName);
                            return Ok(new { status = FriendshipStatus.Accepted });

                        }
                        return Conflict("Already sent Friend request");

                    case FriendshipStatus.Blocked:
                        return Forbid();

                    case FriendshipStatus.Declined:
                        existing.RequesterUserName = me;
                        existing.AddresseeUserName = toUserName;
                        existing.Status = FriendshipStatus.Pending;
                        existing.UpdatedAt = DateTimeOffset.UtcNow;
                        try
                        {
                            await _db.SaveChangesAsync();
                        }
                        catch (DbUpdateException)
                        {
                            return Conflict("Race condition on resend.");

                        }
                        await _hub.Clients.User(toUserName)
                            .SendAsync("FriendRequestIncoming", me);
                        return Ok(new { status = "resent" });
                }
            }
            var fr = new FriendsModel
            {
                RequesterUserName = me,
                AddresseeUserName = toUserName,
                Status = FriendshipStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow,
            };
            _db.Friends.Add(fr);
            await _db.SaveChangesAsync();
            await _hub.Clients.User(toUserName).SendAsync("FriendRequestIncoming", me);
            return Ok(new { status = "sent" });
        }
        public record RespondDto(string FromUserName, bool Accept);

        [HttpPost("respond")]
        public async Task<IActionResult> Respond([FromBody]RespondDto dto)
        {
            var me = User.Identity!.Name!;

            var fr = await _db.Friends.FirstOrDefaultAsync(f => 
            f.RequesterUserName == dto.FromUserName && f.AddresseeUserName == me && f.Status == FriendshipStatus.Pending);
            if (fr == null)
            {
                return NotFound();
            }
            fr.Status = dto.Accept ? FriendshipStatus.Accepted : FriendshipStatus.Declined;
            fr.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();

            await _hub.Clients.User(dto.FromUserName).SendAsync("FriendRequestResult", me, dto.Accept ? "accepted" : "declined");
            await _hub.Clients.User(me).SendAsync("FriendListUpdated");

            return Ok();
        }

        [HttpDelete("{otherUserName}")]
        public async Task<IActionResult> Remove([FromBody] string otherUserName)
        {
            var me = User.Identity!.Name!;
            otherUserName = otherUserName.Trim().ToLowerInvariant() ?? "";

            if (string.IsNullOrWhiteSpace(otherUserName) || otherUserName == me) return BadRequest();

            var fr = await _db.Friends.FirstOrDefaultAsync(f =>
            (f.Status == FriendshipStatus.Accepted) &&
            (f.RequesterUserName == otherUserName && f.AddresseeUserName == me)||
            (f.RequesterUserName == me && f.AddresseeUserName == otherUserName ));

            if (fr == null) return NotFound();

            _db.Friends.Remove(fr);
            await _db.SaveChangesAsync();

            await _hub.Clients.User(otherUserName).SendAsync("FriendRemoved", me);
            await _hub.Clients.User(me).SendAsync("FriendListUpdated");

            return Ok();
        }
    }
}
