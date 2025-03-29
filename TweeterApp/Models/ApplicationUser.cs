using Microsoft.AspNetCore.Identity;

namespace TweeterApp.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FirstName {  get; set; }
        public string LastName {  get; set; }
        public DateTime DateOfBirth {  get; set; }
        public DateTime DateCreated {  get; set; }
        public DateTime DateModified {  get; set; }
        public bool ActiveAccount { get; set; }
        public int GenderId { get; set; }
    }
}
