using Microsoft.AspNetCore.Http;

namespace SummitAPI.Service
{
    public interface IStorageService
    {
        /// <summary>Saves a file and returns a PUBLIC URL (absolute or relative).</summary>
        Task<string> SaveAsync(IFormFile file, CancellationToken ct = default);
    }
}
