using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TweeterApp.Models;
using TweeterApp.Repository;

namespace TweeterApp.Controllers
{
    public class SavedPostsController : Controller
    {
        private readonly ISavedPostsRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;

        public SavedPostsController(ISavedPostsRepository savedPostsRepository, UserManager<ApplicationUser> userManager) 
        {
            _repository = savedPostsRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var savedPosts = await _repository.GetSavedPostsAsync(user.Id);
            return View(savedPosts);
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int postId)
        {
            var user = await _userManager.GetUserAsync(User);
            var isSaved = await _repository.IsSavedAsync(postId, user.Id);

            if (isSaved)
            {
                await _repository.RemoveSavedPostAsync(postId, user.Id);
            }else
            {
                await _repository.SavePostAsync(postId, user.Id);
            }
            return RedirectToAction("Index", "Post");
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int postId)
        {
            var user = await _userManager.GetUserAsync(User);
            await _repository.RemoveSavedPostAsync(postId, user.Id);
            return RedirectToAction("Index");
        }
    }
}
