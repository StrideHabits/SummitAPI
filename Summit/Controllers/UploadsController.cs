using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SummitAPI.Dtos;
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

        // POST /api/uploads  (multipart/form-data with field "file")
        [HttpPost]
        [RequestSizeLimit(10_000_000)] // 10 MB
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<UploadResponse>> Upload([FromForm] UploadFileDto form, CancellationToken ct)
        {
            if (form.File is null || form.File.Length == 0)
                return BadRequest("No file provided.");

            var url = await _storage.SaveAsync(form.File, ct);

            if (url.StartsWith("/"))
                url = $"{Request.Scheme}://{Request.Host}{url}";

            return Ok(new UploadResponse(url));
        }
    }
}
