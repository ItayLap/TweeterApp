namespace TweeterApp.Models
{
    public class FollowModel
    {
        public int Id { get; set; } 
        public int FollowerId{ get; set; }
        public ApplicationUser Follower { get; set; }

        public int FolloweeId {  get; set; }
        public ApplicationUser Followee { get; set; }
    }
}
