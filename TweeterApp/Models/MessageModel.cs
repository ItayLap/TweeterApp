using Microsoft.AspNetCore.Identity;

namespace TweeterApp.Models
{
    public class MessageModel
    {
        public int Id { get; set; }
        public int SenderId {  get; set; }
        public int ReceiverId {  get; set; }

        public string? Subject { get; set; }
        public string Body { set; get; }

        public string ImageUrl { get; set; }
        public string Caption { get; set; }


        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        public ApplicationUser Sender { get; set; }
        public ApplicationUser Receiver { get; set; }
    }
}
