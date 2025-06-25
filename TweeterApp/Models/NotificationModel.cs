namespace TweeterApp.Models
{
    public class NotificationModel
    {
        public int Id { get; set; }
        
        public ApplicationUser Recipiant {  get; set; }
        public int RecipiantId {  get; set; }

        public ApplicationUser Sender {  get; set; }
        public int SenderId { get; set; }

        public string Message {  get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

    }
}
