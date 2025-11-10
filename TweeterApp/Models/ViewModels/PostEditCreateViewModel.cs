using System.ComponentModel.DataAnnotations;

namespace TweeterApp.Models.ViewModels
{
    public class PostEditCreateViewModel
    {
        public int Id { get; set; }
        [Required, StringLength(5000)]
        public string Content { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; }

        public IFormFile? ImageFile { get; set; }
        public string? ExistingImagePath { get; set; }
    }
}
