﻿using System.ComponentModel.DataAnnotations.Schema;
using TweeterApp.Models;

namespace TweeterApp.Models
{
    public class PostModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string Title { get; set; }
        public DateTime CreatedDate { get; set; }

        public string? ImagePath {  get; set; }

        //[ForeignKey(User)]
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }
        public ICollection<CommentModel> Comments { get; set; }

        // public ICollection<LikeModel> Likes { get; set; } = new List<LikeModel>();
    }
}
