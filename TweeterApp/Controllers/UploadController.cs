using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TweeterApp.Controllers
{
    [Authorize]
    public class UploadController : Controller
    {
        [HttpPost("/chat/upload")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("empty");
            }
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
            {
                return BadRequest("unsupported");
            }
            if (!file.ContentType.StartsWith("image/")) return BadRequest("not_image");

            var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploads = Path.Combine(wwwroot, "uploads");
            Directory.CreateDirectory(uploads);

            var name = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploads, name);

            using(var stream = System.IO.File.Create(fullPath)) 
            { 
                await stream.CopyToAsync(stream); 
            }
            var url = $"/uploads/{name}";
            return Ok(new { url });
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
