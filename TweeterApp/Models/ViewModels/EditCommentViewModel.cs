namespace TweeterApp.Models.ViewModels
{
    public class EditCommentViewModel
    {
        public int CommentId { get; set; }
        public int PostId {  get; set; }
        public string Content { get; set; }
    }
}
