namespace TweeterApp.Models
{
    public enum FriendshipStatus : byte
    {
        Pending = 0,
        Accepted = 1,
        Declined = 2,
        Blocked = 3,
    }

    public class FriendsModel
    {
        public int Id {  get; set; }

        public string RequesterUserName { get; set; }
        public string AddresseeUserName { get; set; }
        public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get;set; }
    }
}
