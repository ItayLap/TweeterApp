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
    public class ProfileControllers : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly FollowRepository _followRepository;

        public  ProfileControllers(UserManager<ApplicationUser> userManager, IWebHostEnvironment env, FollowRepository followRepository)
        {
            _userManager = userManager;
            _env = env;
            _followRepository = followRepository;
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

                if (allowedExtensions.Contains(extention))
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
                var filePath = Path.Combine(_env.WebRootPath, fileName);
                using(var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Avatar.CopyToAsync(stream);
                }
                user.AvatarPath = "/uploads/" + fileName;

                await _userManager.UpdateAsync(user);
                return RedirectToAction("MyProfile");
            }
            await _userManager.UpdateAsync(user);
            return RedirectToAction("MyProfile");
        }

    }
}
