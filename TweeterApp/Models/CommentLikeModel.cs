﻿namespace TweeterApp.Models
{
    public class CommentLikeModel
    {
        public int Id { get; set; }

        public ApplicationUser User { get; set; }
        public int UserId { get; set; }

        public int CommentId { get; set; }
        public CommentModel Comment { get; set; }
        
    }
}
