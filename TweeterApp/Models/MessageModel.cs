using Microsoft.AspNetCore.Identity;

namespace TweeterApp.Models
{
    public class MessageModel
    {
        public int Id { get; set; }
        public string SenderId {  get; set; }
        public string ReceiverId {  get; set; }

        public string Subject { get; set; }
        public string Body { set; get; }


        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        public IdentityUser Sender { get; set; }
        public IdentityUser Receiver { get; set; }
    }
}
