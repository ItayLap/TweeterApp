using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SendGrid.Helpers.Mail;
using System.Xml.Linq;
using TweeterApp.Models;
using TweeterApp.Repository;
using TweeterApp.Models.ViewModels;

namespace TweeterApp.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ICommentRepository _commentRepository;
        private readonly UserManager<ApplicationUser> _UserManager;


        public CommentController(ICommentRepository commentRepository, UserManager<ApplicationUser> userManager)
        {
            _commentRepository = commentRepository;
            _UserManager = userManager;
        }
        public async Task<IActionResult> GetComments(int postId)
        {
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
            return RedirectToAction("Details", "Post", new { postId });
        }
        public async Task<IActionResult> DeleteComment(int id)
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

            await _commentRepository.DeleteAsync(comment.Id); //delete atribute comment
            return RedirectToAction("Index", "Post", new {id = comment.PostId});
        }
        [HttpGet]
        public async Task<IActionResult> EditComment(int id)
        {

            var comment = await _commentRepository.GetByIdAsync(id);
            var user = _UserManager.GetUserAsync(User);
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
            };
            return View(model);

        }
        [HttpPost]
        public async Task<IActionResult> EditComment(EditCommentViewModel model)
        {
            var comment = await _commentRepository.GetByIdAsync(model.CommentId);
            var user = _UserManager.GetUserAsync(User);
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
            return RedirectToAction("Details", "Post", new {id = comment.PostId});
        }
    }
}
