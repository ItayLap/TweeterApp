namespace TweeterApp.Models.ViewModels
{
    public class FollowViewModel
    {
        public ApplicationUser User { get; set; }
        public List<ApplicationUser> Followers { get; set; } = new List<ApplicationUser>();
        public List<ApplicationUser> Following { get; set; } = new List<ApplicationUser>();

        public bool IsFollowing {  get; set; }
    }
}
