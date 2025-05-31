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
        public string Bio {  get; set; }
        public string AvatarPath {  get; set; }

        public ICollection<CommentModel> Comments { get; set; }
    }
}
