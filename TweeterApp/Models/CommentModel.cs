namespace TweeterApp.Models
{
    public class CommentModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }

        public int UserId {  get; set; }
        public ApplicationUser User { get; set; }

        public int PostId {  get; set; }
        public PostModel Post { get; set; }
    }
}
