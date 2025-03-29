namespace TweeterApp.Models.ViewModels
{
    public class RegisterViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string FirstName {  get; set; }
        public string LastName {  get; set; }
        public DateTime DateOfBirth { get; set; }
        public int GenderId {  get; set; }
    }
}
