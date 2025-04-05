using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TweeterApp.Models;
using TweeterApp.Repository;

namespace TweeterApp.Controllers
{
    [Authorize]
    public class LikeController : Controller
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IPostRepository _postRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public LikeController(ILikeRepository likeRepository, IPostRepository postRepository, UserManager<ApplicationUser> userManager)
        {
            _likeRepository = likeRepository;
            _postRepository = postRepository;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> LikePost(int PostId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            var post =await _postRepository.GetByIdAsync(PostId);
            if (post == null) 
            {
                return NotFound();
            }
            var isLiked = await _likeRepository.IsLikedAsync(user.Id, PostId);
            if (!isLiked) 
            {
                var like = new LikeModel { UserId = user.Id, PostId = PostId };
                await _likeRepository.AddLikeAsync(like);
            }
            return RedirectToAction("Index", "Post");
        }
        // remove like function with _likeRepository.RemoveLikeAsync(like);
    }
}
