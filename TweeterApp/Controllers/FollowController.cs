using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TweeterApp.Models;
using TweeterApp.Models.ViewModels;
using TweeterApp.Repository;

namespace TweeterApp.Controllers
{
    public class FollowController : Controller
    {
        private readonly IFollowRepository _followRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public FollowController(IFollowRepository followRepository, UserManager<ApplicationUser> userManager)
        {
            _followRepository = followRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var currentUser = await _userManager.GetUserAsync(User);
            if (user == null || currentUser == user)
            {
                return NotFound();
            }
            var followers = await _followRepository.GetFollowersAsync(user.Id);
            var following = await _followRepository.GetFollowingAsync(user.Id);
            var isFollowing = await _followRepository.IsFollowingAsync(currentUser.Id, user.Id);

            var model = new FollowViewModel
            {
                User = user,
                Followers = followers.ToList(),
                Following = following.ToList(),
                IsFollowing = isFollowing
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Follow(int followeeId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Id == followeeId)
            {
                return BadRequest();
            }
            var isFollowing = await _followRepository.IsFollowingAsync(user.Id, followeeId);
            if (!isFollowing)
            {
                await _followRepository.FollowAsync(user.Id, followeeId);
            }
            return RedirectToAction("Index", "User", new {id = followeeId});
        }

        [HttpPost]
        public async Task<IActionResult> UnFollow(int followeeId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Id == followeeId)
            {
                return BadRequest();
            }

            
            await _followRepository.UnfollowAsync(user.Id, followeeId);

            return RedirectToAction("Index", "User", new { userId = followeeId });
        }
    }
}
