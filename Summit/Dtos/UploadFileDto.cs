using Microsoft.AspNetCore.Http;

namespace SummitAPI.Dtos
{
    // Represents the multipart/form-data body: a single file field named "file"
    public class UploadFileDto
    {
        public IFormFile? File { get; set; }
    }
}
