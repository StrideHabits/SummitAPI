using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace SummitAPI.Service
{
    /// <summary>Uploads to Supabase Storage bucket (assumes public bucket).</summary>
    public class SupabaseStorageService : IStorageService
    {
        private readonly HttpClient _http;
        private readonly string _url;
        private readonly string _key;
        private readonly string _bucket;
        private readonly string _publicBase;

        public SupabaseStorageService(HttpClient http, string url, string key, string bucket, string publicUrlBase)
        {
            _http = http;
            _url = url.TrimEnd('/');
            _key = key;
            _bucket = bucket;
            _publicBase = publicUrlBase.TrimEnd('/');
        }

        public async Task<string> SaveAsync(IFormFile file, CancellationToken ct = default)
        {
            var ext = Path.GetExtension(file.FileName);
            var path = $"habits/{Guid.NewGuid():N}{ext}";
            using var content = new StreamContent(file.OpenReadStream());
            content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            var req = new HttpRequestMessage(HttpMethod.Post,
                $"{_url}/storage/v1/object/{_bucket}/{path}")
            { Content = content };

            // Supabase requires both Authorization and apikey headers
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _key);
            req.Headers.Add("apikey", _key);

            var res = await _http.SendAsync(req, ct);
            res.EnsureSuccessStatusCode();

            // Public URL (bucket must be public)
            return $"{_publicBase}/{_bucket}/{path}";
        }
    }
}
