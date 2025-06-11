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
        private readonly IPostRepository _postRepository;

        public CommentController(ICommentRepository commentRepository, UserManager<ApplicationUser> userManager, IPostRepository postRepository)
        {
            _commentRepository = commentRepository;
            _UserManager = userManager;
            _postRepository = postRepository;
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
        public async Task<IActionResult> DeleteComment(int commentId,int postId)
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
            return RedirectToAction("Index", "Post", new {postId = comment.PostId});
        }
        [HttpGet]
        public async Task<IActionResult> EditComment(int id)
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
            };
            return View(model);

        }
        [HttpPost]
        public async Task<IActionResult> EditComment(EditCommentViewModel model)
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
            return RedirectToAction("Details", "Post", new {postId = model.PostId});
        }
    }
}
