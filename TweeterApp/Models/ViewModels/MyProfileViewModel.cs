namespace TweeterApp.Models.ViewModels
{
    public class MyProfileViewModel
    {
        public ApplicationUser User { get; set; }
        public List<ApplicationUser> Followers { get; set; }
        public List<ApplicationUser> Following { get; set; }

        public bool isFollowing { get; set; }
    }
}
