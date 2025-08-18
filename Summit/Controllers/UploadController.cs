using System.Security.Claims;
using Summit.Data;
using Summit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Summit.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly string _storagePath = "/data/uploads";
        public UploadController(AppDbContext db)
        {
            _db = db;
            Directory.CreateDirectory(_storagePath);
        }

        private int CurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // multipart/form-data: file + completionId
        [HttpPost]
        [RequestSizeLimit(20_000_000)] // 20 MB
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] int completionId)
        {
            var uid = CurrentUserId();
            var comp = await _db.Completions.Include(c => c.Habit)
                .FirstOrDefaultAsync(c => c.Id == completionId && c.Habit.UserId == uid);
            if (comp == null) return BadRequest("Invalid completion.");

            if (file == null || file.Length == 0) return BadRequest("No file.");

            var safeName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(_storagePath, safeName);
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var publicUrl = $"/uploads/{safeName}";
            var attach = new Attachment { CompletionId = comp.Id, FileUrl = publicUrl };
            _db.Attachments.Add(attach);
            await _db.SaveChangesAsync();

            return Ok(new { url = publicUrl, attachmentId = attach.Id });
        }
    }
}
