namespace TweeterApp.Models.ViewModels
{
    public class ProfileEditViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Bio { get; set; }

        public IFormFile Avatar { get; set; }
        public string CurrentAvatarPath {  get; set; }
    }
}
