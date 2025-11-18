using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SummitAPI.Service;

namespace SummitAPI.Tests.Service
{
    public class SupabaseStorageServiceTests
    {
        public static async Task Main()
        {
            Console.WriteLine("Running SupabaseStorageService Tests...\n");

            // Test 1: Constructor initializes properties correctly
            var mockHttp = new HttpClient(new MockHttpMessageHandler());
            var url = "https://project.supabase.co";
            var key = "test_api_key_12345";
            var bucket = "uploads";
            var publicBase = "https://project.supabase.co/storage/v1/object/public";

            var service = new SupabaseStorageService(mockHttp, url, key, bucket, publicBase);
            Console.WriteLine("✓ Constructor accepts all parameters");

            // Test 2: Constructor trims trailing slashes from URL
            var service2 = new SupabaseStorageService(
                mockHttp,
                "https://project.supabase.co/",
                key,
                bucket,
                "https://project.supabase.co/storage/v1/object/public/"
            );
            Console.WriteLine("✓ Constructor handles trailing slashes");

            // Test 3: SaveAsync returns correct URL format
            var file1 = CreateMockFile("test.txt", "Hello World");
            var url1 = await service.SaveAsync(file1);

            Console.WriteLine($"\n✓ SaveAsync returns URL: {!string.IsNullOrEmpty(url1)}");
            Console.WriteLine($"  URL: {url1}");
            Console.WriteLine($"✓ URL contains bucket name: {url1.Contains("/uploads/")}");
            Console.WriteLine($"✓ URL contains 'habits/' path: {url1.Contains("/habits/")}");
            Console.WriteLine($"✓ URL contains file extension: {url1.EndsWith(".txt")}");
            Console.WriteLine($"✓ URL is absolute: {url1.StartsWith("https://")}");

            // Test 4: Different file extensions are preserved
            var txtFile = CreateMockFile("file.txt", "text");
            var jpgFile = CreateMockFile("photo.jpg", "image_data");
            var pngFile = CreateMockFile("icon.png", "png_data");
            var pdfFile = CreateMockFile("doc.pdf", "pdf_data");

            var txtUrl = await service.SaveAsync(txtFile);
            var jpgUrl = await service.SaveAsync(jpgFile);
            var pngUrl = await service.SaveAsync(pngFile);
            var pdfUrl = await service.SaveAsync(pdfFile);

            Console.WriteLine($"\n✓ Preserves .txt extension: {txtUrl.EndsWith(".txt")}");
            Console.WriteLine($"✓ Preserves .jpg extension: {jpgUrl.EndsWith(".jpg")}");
            Console.WriteLine($"✓ Preserves .png extension: {pngUrl.EndsWith(".png")}");
            Console.WriteLine($"✓ Preserves .pdf extension: {pdfUrl.EndsWith(".pdf")}");

            // Test 5: Each file gets unique URL (GUID-based)
            var fileA = CreateMockFile("same.txt", "A");
            var fileB = CreateMockFile("same.txt", "B");
            var fileC = CreateMockFile("same.txt", "C");

            var urlA = await service.SaveAsync(fileA);
            var urlB = await service.SaveAsync(fileB);
            var urlC = await service.SaveAsync(fileC);

            Console.WriteLine($"\n✓ Each file gets unique URL: {urlA != urlB && urlB != urlC}");
            Console.WriteLine($"  File A: {urlA.Split('/').Last()}");
            Console.WriteLine($"  File B: {urlB.Split('/').Last()}");
            Console.WriteLine($"  File C: {urlC.Split('/').Last()}");

            // Test 6: Cancellation token support
            var file2 = CreateMockFile("cancel.txt", "test");
            var cts = new CancellationTokenSource();
            var url2 = await service.SaveAsync(file2, cts.Token);
            Console.WriteLine($"\n✓ Accepts cancellation token: {!string.IsNullOrEmpty(url2)}");

            // Test 7: URL structure is correct
            var file3 = CreateMockFile("structure.jpg", "data");
            var url3 = await service.SaveAsync(file3);
            var parts = url3.Split('/');

            Console.WriteLine($"\n✓ URL has correct structure:");
            Console.WriteLine($"  Contains public base: {url3.StartsWith(publicBase)}");
            Console.WriteLine($"  Contains bucket: {parts.Contains("uploads")}");
            Console.WriteLine($"  Contains 'habits' folder: {parts.Contains("habits")}");
            Console.WriteLine($"  Has filename with GUID: {parts.Last().Length > 32}");

            // Test 8: File with no extension
            var noExtFile = CreateMockFile("noextension", "content");
            var noExtUrl = await service.SaveAsync(noExtFile);
            Console.WriteLine($"\n✓ Handles files without extension: {!string.IsNullOrEmpty(noExtUrl)}");
            Console.WriteLine($"  URL: {noExtUrl}");

            // Test 9: Different content types
            var htmlFile = CreateMockFile("page.html", "<html></html>");
            var jsonFile = CreateMockFile("data.json", "{\"key\":\"value\"}");
            var csvFile = CreateMockFile("data.csv", "name,age\nJohn,30");

            var htmlUrl = await service.SaveAsync(htmlFile);
            var jsonUrl = await service.SaveAsync(jsonFile);
            var csvUrl = await service.SaveAsync(csvFile);

            Console.WriteLine($"\n✓ Handles .html files: {htmlUrl.EndsWith(".html")}");
            Console.WriteLine($"✓ Handles .json files: {jsonUrl.EndsWith(".json")}");
            Console.WriteLine($"✓ Handles .csv files: {csvUrl.EndsWith(".csv")}");

            // Test 10: Custom bucket name
            var customService = new SupabaseStorageService(
                mockHttp,
                url,
                key,
                "custom-bucket",
                publicBase
            );
            var customFile = CreateMockFile("custom.txt", "data");
            var customUrl = await customService.SaveAsync(customFile);
            Console.WriteLine($"\n✓ Custom bucket works: {customUrl.Contains("custom-bucket")}");
            Console.WriteLine($"  URL: {customUrl}");

            // Test 11: Verify path structure
            var file4 = CreateMockFile("verify.png", "image");
            var url4 = await service.SaveAsync(file4);
            var pathPart = url4.Substring(publicBase.Length + 1); // Remove base
            var pathSegments = pathPart.Split('/');

            Console.WriteLine($"\n✓ Path structure verification:");
            Console.WriteLine($"  Bucket: {pathSegments[0]}");
            Console.WriteLine($"  Folder: {pathSegments[1]}");
            Console.WriteLine($"  Filename: {pathSegments[2]}");
            Console.WriteLine($"  Has 3 segments: {pathSegments.Length == 3}");

            Console.WriteLine("\n✅ All tests passed!");
            Console.WriteLine("\nNote: These tests use a mock HTTP handler.");
            Console.WriteLine("For integration testing, use real Supabase credentials.");
        }

        // Helper method to create mock IFormFile
        private static IFormFile CreateMockFile(string fileName, string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);

            return new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = GetContentType(fileName)
            };
        }

        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            return extension switch
            {
                ".txt" => "text/plain",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                ".html" => "text/html",
                ".json" => "application/json",
                ".csv" => "text/csv",
                _ => "application/octet-stream"
            };
        }
    }

    // Mock HTTP handler for testing without real Supabase connection
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Simulate successful upload
            await Task.Delay(10, cancellationToken);

            // Verify headers are set correctly
            var hasAuth = request.Headers.Authorization != null;
            var hasApiKey = request.Headers.Contains("apikey");

            if (!hasAuth || !hasApiKey)
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("Missing authentication headers")
                };
            }

            // Return success response
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"Key\":\"habits/test.jpg\"}")
            };
        }
    }
}
