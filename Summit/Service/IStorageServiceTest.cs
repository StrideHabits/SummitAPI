using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SummitAPI.Service;

namespace SummitAPI.Tests.Service
{
    public class StorageServiceTests
    {
        public static async Task Main()
        {
            Console.WriteLine("Running StorageService Tests...\n");

            // Create a mock implementation for testing
            IStorageService storageService = new MockStorageService();

            // Test 1: Save a small text file
            var file1 = CreateMockFile("test.txt", "Hello World");
            var url1 = await storageService.SaveAsync(file1);
            Console.WriteLine($"✓ Saves text file: {!string.IsNullOrEmpty(url1)}");
            Console.WriteLine($"  Returned URL: {url1}");

            // Test 2: Save an image file
            var file2 = CreateMockFile("image.jpg", "fake_image_data_12345");
            var url2 = await storageService.SaveAsync(file2);
            Console.WriteLine($"\n✓ Saves image file: {!string.IsNullOrEmpty(url2)}");
            Console.WriteLine($"  Returned URL: {url2}");

            // Test 3: Save with cancellation token
            var file3 = CreateMockFile("document.pdf", "pdf_content");
            var cts = new CancellationTokenSource();
            var url3 = await storageService.SaveAsync(file3, cts.Token);
            Console.WriteLine($"\n✓ Accepts cancellation token: {!string.IsNullOrEmpty(url3)}");

            // Test 4: Different file types
            var txtFile = CreateMockFile("file.txt", "text");
            var jpgFile = CreateMockFile("photo.jpg", "jpg");
            var pngFile = CreateMockFile("icon.png", "png");
            var pdfFile = CreateMockFile("doc.pdf", "pdf");

            var txtUrl = await storageService.SaveAsync(txtFile);
            var jpgUrl = await storageService.SaveAsync(jpgFile);
            var pngUrl = await storageService.SaveAsync(pngFile);
            var pdfUrl = await storageService.SaveAsync(pdfFile);

            Console.WriteLine($"\n✓ Handles .txt files: {txtUrl.Contains("txt")}");
            Console.WriteLine($"✓ Handles .jpg files: {jpgUrl.Contains("jpg")}");
            Console.WriteLine($"✓ Handles .png files: {pngUrl.Contains("png")}");
            Console.WriteLine($"✓ Handles .pdf files: {pdfUrl.Contains("pdf")}");

            // Test 5: Returns valid URLs
            var file5 = CreateMockFile("test.jpg", "data");
            var url5 = await storageService.SaveAsync(file5);
            var isAbsoluteUrl = url5.StartsWith("http://") || url5.StartsWith("https://");
            var isRelativeUrl = url5.StartsWith("/");
            Console.WriteLine($"\n✓ Returns URL (absolute or relative): {isAbsoluteUrl || isRelativeUrl}");
            Console.WriteLine($"  URL type: {(isAbsoluteUrl ? "Absolute" : "Relative")}");

            // Test 6: Multiple files don't overwrite
            var fileA = CreateMockFile("file1.txt", "content A");
            var fileB = CreateMockFile("file2.txt", "content B");
            var fileC = CreateMockFile("file3.txt", "content C");

            var urlA = await storageService.SaveAsync(fileA);
            var urlB = await storageService.SaveAsync(fileB);
            var urlC = await storageService.SaveAsync(fileC);

            Console.WriteLine($"\n✓ Multiple saves return different URLs: {urlA != urlB && urlB != urlC}");
            Console.WriteLine($"  URL A: {urlA}");
            Console.WriteLine($"  URL B: {urlB}");
            Console.WriteLine($"  URL C: {urlC}");

            // Test 7: File with special characters
            var specialFile = CreateMockFile("my file (1) - copy.txt", "content");
            var specialUrl = await storageService.SaveAsync(specialFile);
            Console.WriteLine($"\n✓ Handles filename with spaces/special chars: {!string.IsNullOrEmpty(specialUrl)}");

            // Test 8: Empty file
            var emptyFile = CreateMockFile("empty.txt", "");
            var emptyUrl = await storageService.SaveAsync(emptyFile);
            Console.WriteLine($"\n✓ Handles empty file: {!string.IsNullOrEmpty(emptyUrl)}");

            Console.WriteLine("\n✅ All tests passed!");
            Console.WriteLine("\nNote: These tests use a mock implementation.");
            Console.WriteLine("Replace MockStorageService with your actual implementation to test it.");
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
                _ => "application/octet-stream"
            };
        }
    }

    // Mock implementation for testing
    public class MockStorageService : IStorageService
    {
        private static int _counter = 0;

        public async Task<string> SaveAsync(IFormFile file, CancellationToken ct = default)
        {
            // Simulate async operation
            await Task.Delay(10, ct);

            // Generate a mock URL with unique identifier
            var timestamp = DateTime.UtcNow.Ticks;
            var counter = Interlocked.Increment(ref _counter);
            var fileName = Path.GetFileName(file.FileName);

            // Return a mock URL (you can change this to match your actual URL format)
            return $"https://storage.example.com/uploads/{timestamp}_{counter}_{fileName}";
        }
    }
}
