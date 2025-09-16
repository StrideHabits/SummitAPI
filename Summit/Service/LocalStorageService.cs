using Microsoft.AspNetCore.Http;

namespace SummitAPI.Service
{
    /// <summary>Stores files on Render disk (/data/uploads) and serves via /uploads/*</summary>
    public class LocalStorageService : IStorageService
    {
        private readonly string _root;
        private readonly string _publicBasePath; // e.g., "/uploads"

        public LocalStorageService(string root, string publicBasePath)
        {
            _root = root;
            _publicBasePath = publicBasePath.StartsWith("/") ? publicBasePath : "/" + publicBasePath;
            Directory.CreateDirectory(_root);
        }

        public async Task<string> SaveAsync(IFormFile file, CancellationToken ct = default)
        {
            var ext = Path.GetExtension(file.FileName);
            var name = $"{Guid.NewGuid():N}{ext}";
            var full = Path.Combine(_root, name);

            using (var fs = new FileStream(full, FileMode.CreateNew))
                await file.CopyToAsync(fs, ct);

            // return relative URL (controller can make absolute as needed)
            return $"{_publicBasePath}/{name}";
        }
    }
}
