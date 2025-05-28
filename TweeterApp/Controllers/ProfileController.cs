using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using TweeterApp.Models;
using TweeterApp.Models.ViewModels;
using TweeterApp.Repository;

namespace TweeterApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly IFollowRepository _followRepository;
        private readonly ILogger<ProfileController> _logger;

        public  ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment env, IFollowRepository followRepository, ILogger<ProfileController> logger)
        {
            _userManager = userManager;
            _env = env;
            _followRepository = followRepository;
            _logger = logger;
        }

        public async Task<IActionResult> ViewProfile(int userId)
        {
            var CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser != null && CurrentUser.Id == userId)
            {
                _logger.LogInformation("shound go to MyProfile");
                return RedirectToAction("MyProfile");
            }

            var user  = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound();
            }
            var followers = await _followRepository.GetFollowersAsync(user.Id);
            var following = await _followRepository.GetFollowingAsync(user.Id);
            var isFollowing = await _followRepository.IsFollowingAsync(CurrentUser.Id,user.Id);
            var model = new MyProfileViewModel
            {
                User = user,
                Followers = followers.ToList(),
                Following = following.ToList(),
                isFollowing = isFollowing
            };
            _logger.LogInformation("passed model creation");
            return View("Profile", model);
        }

        public async Task<IActionResult> MyProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            var followers = await _followRepository.GetFollowingAsync(user.Id);
            var following = await _followRepository.GetFollowingAsync(user.Id);
            if (user == null)
            {
                _logger.LogInformation("null user");
                return RedirectToAction("Login", "Account");
            }
            var model = new MyProfileViewModel
            {
                User = user,
                Followers = followers.ToList(),
                Following = following.ToList(),
            };
            _logger.LogInformation("passed model creation for MyProfile");
            return View(model);
        }

        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) 
            {
                return RedirectToAction("Login", "Account");
            }
            var model = new ProfileEditViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Bio = user.Bio,
                CurrentAvatarPath = user.AvatarPath
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(ProfileEditViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Bio = model.Bio;
            if (model.Avatar != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var maxFileSizeInBytes = 2 * 1024 * 1024; //2 MB
                var extention = Path.GetExtension(model.Avatar.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extention))
                {
                    ModelState.AddModelError("Avatar", "only .jpg / .jpeg / .png / .gif files are allowed");
                    return View(model);
                }
                if (model.Avatar.Length > maxFileSizeInBytes)
                {
                    ModelState.AddModelError("Avatar", "file size must be less then 2MB");
                    return View(model);
                }

                if (!string.IsNullOrEmpty(user.AvatarPath) && !user.AvatarPath.Contains("default"))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, user.AvatarPath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                var uploadsPath = Path.Combine(_env.WebRootPath, "Uploads");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }
                var fileName = Guid.NewGuid().ToString()+ Path.GetExtension(model.Avatar.FileName);
                var filePath = Path.Combine(uploadsPath, fileName);
                using(var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Avatar.CopyToAsync(stream);
                }
                user.AvatarPath = "/Uploads/" + fileName;
            }
            await _userManager.UpdateAsync(user);
            return RedirectToAction("MyProfile");
        }

    }
}
