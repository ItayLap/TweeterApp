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
        public async Task<IActionResult> Create(PostEditCreateViewModel Post)
        {
            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    var key = entry.Key;
                    //var errors = entry.Value.Errors;

                    foreach (var err in entry.Value.Errors)
                    {
                        _logger.LogWarning("ModelState Error for '{Field}' :{Message}", key ?? "(null)", err.ErrorMessage ?? "(none)");
                    }
                }
                _logger.LogWarning("Register model invalid");
                return View(Post);
            }
            string? imagePath = null;
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Forbid();
                }
                if(Post.ImageFile != null && Post.ImageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(Post.ImageFile.FileName);
                    var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    Directory.CreateDirectory(uploadsPath);
                    var filePath = Path.Combine(uploadsPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Post.ImageFile.CopyToAsync(stream);
                    }
                    imagePath = "/Uploads/" + fileName;
                }

                var post = new PostModel
                {
                    Title = Post.Title,
                    Content = Post.Content,
                    CreatedDate = DateTime.UtcNow,
                    UserId = int.Parse(_userManager.GetUserId(User)!),
                    ImagePath = imagePath
                };

                await _postRepository.AddAsync(post);
                return RedirectToAction("Index");
            }
            return View(Post);
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
        private static bool IsAllowedImageExtension(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext is ".jpg" or ".jpeg" or ".png" or ".gif";
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PostEditCreateViewModel model)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null) return NotFound();
            if (!(IsAdmin() || post.UserId == CurrentUserId())) return Forbid();

            if (!ModelState.IsValid) return View(model);

            if (model.ImageFile is { Length: > 0 })
            {
                if (!IsAllowedImageExtension(model.ImageFile.FileName))
                    ModelState.AddModelError(nameof(model.ImageFile), "Only .jpg / .jpeg / .png / .gif files are allowed");
                if (model.ImageFile.Length > 2 * 1024 * 1024)
                    ModelState.AddModelError(nameof(model.ImageFile), "File size must be ≤ 2 MB");
                if (!ModelState.IsValid) return View(model);
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                Directory.CreateDirectory(uploads);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ImageFile.FileName)}";
                await using var fs = new FileStream(Path.Combine(uploads, fileName), FileMode.Create);
                await model.ImageFile.CopyToAsync(fs);
                post.ImagePath = "/Uploads/" + fileName;
            }

            post.Title = model.Title;
            post.Content = model.Content;
            //post.DateModified = DateTime.UtcNow;

            await _postRepository.UpdateAsync(post);
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            var vm = new PostEditCreateViewModel
            {
                Id = id,
                Title = post.Title,
                Content = post.Content,
                ExistingImagePath = post.ImagePath
            };
            return View(vm);
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
