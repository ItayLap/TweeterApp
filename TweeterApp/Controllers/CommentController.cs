﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SendGrid.Helpers.Mail;
using System.Xml.Linq;
using TweeterApp.Models;
using TweeterApp.Repository;
using TweeterApp.Models.ViewModels;
using TweeterApp.Data;
using Microsoft.EntityFrameworkCore;

namespace TweeterApp.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ICommentRepository _commentRepository;
        private readonly UserManager<ApplicationUser> _UserManager;
        private readonly IPostRepository _postRepository;
        private readonly ApplicationDbContext _context;

        public CommentController(ApplicationDbContext context,ICommentRepository commentRepository, UserManager<ApplicationUser> userManager, IPostRepository postRepository)
        {
            _commentRepository = commentRepository;
            _UserManager = userManager;
            _postRepository = postRepository;
            _context = context;
        }
        public async Task<IActionResult> GetComments(int postId)
        {
            var post = _commentRepository.GetByIdAsync(postId);
            if (post == null)
            {
                return NotFound();
            }
            var comments = await _commentRepository.GetByPostIdAsync(postId);
            return PartialView("_CommentsPartial", comments);
        }
        [HttpPost]
        public async Task<IActionResult> AddComment(int postId, string content) 
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError("", "No empty comments allowed");
                return BadRequest(ModelState);
            }
            var user = await _UserManager.GetUserAsync(User);
            var Comment = new CommentModel
            {
                PostId = postId,
                Content = content,
                CreatedDate = DateTime.UtcNow,
                UserId = user.Id
            };
            
            await _commentRepository.AddAsync(Comment);
            return RedirectToAction("Details", "Post", new {id = postId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int commentId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            var user = await _UserManager.GetUserAsync(User);

            if (comment == null)
            {
                return NotFound();
            }

            if (user == null || comment.UserId != user.Id)
            {
                return Forbid();
            }

            await _commentRepository.DeleteAsync(commentId); //delete atribute comment
            return RedirectToAction("Details", "Post", new {id = comment.PostId});
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            var user = await _UserManager.GetUserAsync(User);
            if (comment == null)
            {
                return NotFound();
            }
            if (user == null || comment.UserId != user.Id)
            {
                return Forbid();
            }
            return View(comment);
        }
        
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {

            var comment = await _commentRepository.GetByIdAsync(id);
            var user = await _UserManager.GetUserAsync(User);
            if (comment == null)
            {
                return NotFound();
            }

            if (user == null || comment.UserId != user.Id)
            {
                return Forbid();
            }
            var model = new EditCommentViewModel
            {
                CommentId = comment.Id,
                Content = comment.Content,
                PostId = comment.PostId,
            };
            return View(model);

        }
        [HttpPost]
        public async Task<IActionResult> Edit(EditCommentViewModel model)
        {
            var comment = await _commentRepository.GetByIdAsync(model.CommentId);
            var user = await _UserManager.GetUserAsync(User);
            if (comment == null)
            {
                return NotFound();
            }

            if (user == null || comment.UserId != user.Id)
            {
                return Forbid();
            }
            comment.Content = model.Content;
            await _commentRepository.UpdateAsync(comment);
            return RedirectToAction("Details", "Post", new {id = model.PostId});
        }


        [HttpPost]
        public async Task<IActionResult> ToggleLike(int commentId , int postId)
        {
            var user = await _UserManager.GetUserAsync(User);
            if (user == null)
            {
                return Forbid();
            }

            bool liked = await _commentRepository.ToggleLikeAsync(commentId, user.Id);
            if (liked)
            {
                TempData["Notification"] = "liked";
            }
            else
            {
                TempData["Notification"] = "like removed";
            }

            return RedirectToAction("Details","Post",new {id = postId});
        }
    }
}
