namespace TweeterApp.Models.ViewModels
{
    public class PostViewModel
    {
        public PostModel Post { get; set; }
        public int LikeCount{ get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public bool IsSavedByCurrentUser { get; set; }
        public IEnumerable<CommentModel> Comments { get; set; }
    }
}
