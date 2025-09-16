using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SummitAPI.Service;

namespace SummitAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class UploadsController : ControllerBase
    {
        private readonly IStorageService _storage;
        public UploadsController(IStorageService storage) => _storage = storage;

        // POST /api/uploads  (multipart/form-data with "file")
        [HttpPost]
        [RequestSizeLimit(10_000_000)] // 10 MB
        public async Task<IActionResult> Upload([FromForm] IFormFile file, CancellationToken ct)
        {
            if (file is null || file.Length == 0) return BadRequest("No file provided.");
            var url = await _storage.SaveAsync(file, ct);

            // If relative path returned, turn into absolute for convenience:
            if (url.StartsWith("/"))
                url = $"{Request.Scheme}://{Request.Host}{url}";

            return Ok(new { url });
        }
    }
}
