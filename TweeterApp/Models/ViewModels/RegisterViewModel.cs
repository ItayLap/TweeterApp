using System.ComponentModel.DataAnnotations;

namespace TweeterApp.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password",ErrorMessage ="Paswords do not match")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string FirstName {  get; set; }

        [Required]
        public string LastName {  get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public int GenderId {  get; set; }

        [Required]
        public IFormFile Avatar { get; set; }
    }
}
