using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Globalization;
using TweeterApp.Models;
using TweeterApp.Models.ViewModels;
using TweeterApp.Repository;

namespace TweeterApp.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        public readonly IPostRepository _postRepository;
        public readonly UserManager<ApplicationUser> _userManager;
        public readonly ILogger<PostController> _logger;
        public readonly ILikeRepository _likeRepository;
        public readonly ICommentRepository _commentRepository;
        public readonly ISavedPostsRepository _savedPostsRepository;

        private int CurrentUserId() => int.Parse(_userManager.GetUserId(User)!);
        private bool IsAdmin() => User.IsInRole("Admin");

        public PostController(IPostRepository postRepository, UserManager<ApplicationUser> userManager, ILogger<PostController> logger, ILikeRepository likeRepository, ICommentRepository commentRepository, ISavedPostsRepository savedPostsRepository)
        {
            _postRepository = postRepository;
            _userManager = userManager;
            _logger = logger;
            _likeRepository = likeRepository;
            _commentRepository = commentRepository;
            _savedPostsRepository = savedPostsRepository;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _postRepository.GetAllAsync();
            var user = await _userManager.GetUserAsync(User);
            var result = new List<PostViewModel>();
            foreach (var post in posts)
            {
                var isLiked = await _likeRepository.IsLikedAsync(user.Id, post.Id);
                var likeCount = await _likeRepository.GetLikeCountAsync(post.Id);
                var isSaved = await _savedPostsRepository.IsSavedAsync(post.Id, user.Id);

                result.Add(new PostViewModel {
                    Post = post,
                    IsLikedByCurrentUser = isLiked,
                    IsSavedByCurrentUser = isSaved,
                    LikeCount = likeCount
                });
            }
            return View(result);
        }

        public IActionResult Create() 
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostModel Post, IFormFile imageFile)
        {
            //if (ModelState.IsValid)
            //{
                var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Forbid();
            }
            if(imageFile != null && imageFile.Length > 0)
            {
                var fileName = Path.GetFileName(imageFile.FileName);
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(uploadsPath);
                var filePath = Path.Combine(uploadsPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                Post.ImagePath = "/Uploads/" + fileName;
            }

            var post = new PostModel
            {
                Title = Post.Title,
                Content = Post.Content,
                CreatedDate = DateTime.UtcNow,
                UserId = user.Id,
                ImagePath = Post.ImagePath
            };

            await _postRepository.AddAsync(post);
            return RedirectToAction("Index");
            //}
           // return View(Post);
        }

        public async Task<IActionResult> GetPost(int id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            var user = await _userManager.GetUserAsync(User);
            if (post == null || post.UserId != user.Id)
            {
                return Forbid();
            }
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PostModel Post, IFormFile imageFile)
        {
            if (id != Post.Id) return NotFound();
            var user = await _userManager.GetUserAsync(User);
            if (Post.UserId != user.Id)
            {
                _logger.LogInformation("post.UserId = {postUserId}, user.Id = {UserId}", Post.UserId, user.Id);
                return Forbid();
            }
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Path.GetFileName(imageFile.FileName);
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(uploadsPath);
                var filePath = Path.Combine(uploadsPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                Post.ImagePath = "/Uploads/" + fileName;
            }
            //if (ModelState.IsValid)
            //{
            await _postRepository.UpdateAsync(Post);
                return RedirectToAction("Index");
           // }
           //return View(post);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
            {
                return Forbid();
            }
            var isOwner = post.UserId == CurrentUserId();
            if (!isOwner && !IsAdmin())
            {
                return Forbid();
            }
            return View(post);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //[Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            var isOwner = post.UserId == CurrentUserId();

            //if (post.UserId != int.Parse(_userManager.GetUserId(User))) return Forbid();
            if (!isOwner && !IsAdmin())
            {
                return Forbid();
            }

            var comments = await _commentRepository.GetByPostIdAsync(id);
            foreach (var comment in comments)
            {
                await _commentRepository.DeleteAsync(comment.Id);
            }

            await _postRepository.DeleteAsync(id);
            return RedirectToAction("Index");

        }
        public async Task<IActionResult> Details(int id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(User);
            var isLiked = await _likeRepository.IsLikedAsync(user.Id, post.Id);
            var LikeCount = await _likeRepository.GetLikeCountAsync(post.Id);

            var comments = await _commentRepository.GetByPostIdAsync(id);

            var model = new PostDetailsViewModel
            {
                Post = post,
                IsLikedByCurrentUser = isLiked,
                LikeCount = LikeCount,
                Comments = comments
            };
            return View(model);
        }
    }
}
