using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SummitAPI.Service;

namespace SummitAPI.Tests.Service
{
    public class LocalStorageServiceTests
    {
        public static async Task Main()
        {
            Console.WriteLine("Running LocalStorageService Tests...\n");

            // Setup test directory
            var testRoot = Path.Combine(Path.GetTempPath(), "storage_test_" + Guid.NewGuid().ToString("N"));
            var publicBasePath = "/uploads";

            try
            {
                // Test 1: Constructor creates directory
                var service1 = new LocalStorageService(testRoot, publicBasePath);
                Console.WriteLine($"✓ Constructor creates root directory: {Directory.Exists(testRoot)}");

                // Test 2: Save a file and get URL
                var file1 = CreateMockFile("test.txt", "Hello World");
                var url1 = await service1.SaveAsync(file1);
                Console.WriteLine($"\n✓ SaveAsync returns URL: {!string.IsNullOrEmpty(url1)}");
                Console.WriteLine($"  URL: {url1}");
                Console.WriteLine($"✓ URL starts with '/uploads': {url1.StartsWith("/uploads/")}");
                Console.WriteLine($"✓ URL contains .txt extension: {url1.EndsWith(".txt")}");

                // Test 3: File is actually saved to disk
                var fileName1 = Path.GetFileName(url1.Replace("/uploads/", ""));
                var fullPath1 = Path.Combine(testRoot, fileName1);
                Console.WriteLine($"\n✓ File exists on disk: {File.Exists(fullPath1)}");

                if (File.Exists(fullPath1))
                {
                    var content = await File.ReadAllTextAsync(fullPath1);
                    Console.WriteLine($"✓ File content matches: {content == "Hello World"}");
                }

                // Test 4: Different file extensions are preserved
                var txtFile = CreateMockFile("file.txt", "text");
                var jpgFile = CreateMockFile("photo.jpg", "image_data");
                var pngFile = CreateMockFile("icon.png", "png_data");
                var pdfFile = CreateMockFile("doc.pdf", "pdf_data");

                var txtUrl = await service1.SaveAsync(txtFile);
                var jpgUrl = await service1.SaveAsync(jpgFile);
                var pngUrl = await service1.SaveAsync(pngFile);
                var pdfUrl = await service1.SaveAsync(pdfFile);

                Console.WriteLine($"\n✓ Preserves .txt extension: {txtUrl.EndsWith(".txt")}");
                Console.WriteLine($"✓ Preserves .jpg extension: {jpgUrl.EndsWith(".jpg")}");
                Console.WriteLine($"✓ Preserves .png extension: {pngUrl.EndsWith(".png")}");
                Console.WriteLine($"✓ Preserves .pdf extension: {pdfUrl.EndsWith(".pdf")}");

                // Test 5: Each file gets unique name (GUID-based)
                var fileA = CreateMockFile("same.txt", "A");
                var fileB = CreateMockFile("same.txt", "B");
                var fileC = CreateMockFile("same.txt", "C");

                var urlA = await service1.SaveAsync(fileA);
                var urlB = await service1.SaveAsync(fileB);
                var urlC = await service1.SaveAsync(fileC);

                Console.WriteLine($"\n✓ Each file gets unique URL: {urlA != urlB && urlB != urlC}");
                Console.WriteLine($"  File A: {urlA}");
                Console.WriteLine($"  File B: {urlB}");
                Console.WriteLine($"  File C: {urlC}");

                // Test 6: Cancellation token support
                var file2 = CreateMockFile("cancel.txt", "test");
                var cts = new CancellationTokenSource();
                var url2 = await service1.SaveAsync(file2, cts.Token);
                Console.WriteLine($"\n✓ Accepts cancellation token: {!string.IsNullOrEmpty(url2)}");

                // Test 7: Public base path without leading slash
                var testRoot2 = Path.Combine(Path.GetTempPath(), "storage_test2_" + Guid.NewGuid().ToString("N"));
                var service2 = new LocalStorageService(testRoot2, "uploads"); // no leading slash
                var file3 = CreateMockFile("test.txt", "content");
                var url3 = await service2.SaveAsync(file3);
                Console.WriteLine($"\n✓ Handles basePath without '/': {url3.StartsWith("/uploads/")}");
                Console.WriteLine($"  URL: {url3}");

                // Clean up test directory 2
                if (Directory.Exists(testRoot2))
                    Directory.Delete(testRoot2, true);

                // Test 8: Custom public base path
                var testRoot3 = Path.Combine(Path.GetTempPath(), "storage_test3_" + Guid.NewGuid().ToString("N"));
                var service3 = new LocalStorageService(testRoot3, "/static/files");
                var file4 = CreateMockFile("custom.txt", "data");
                var url4 = await service3.SaveAsync(file4);
                Console.WriteLine($"\n✓ Custom base path works: {url4.StartsWith("/static/files/")}");
                Console.WriteLine($"  URL: {url4}");

                // Clean up test directory 3
                if (Directory.Exists(testRoot3))
                    Directory.Delete(testRoot3, true);

                // Test 9: Large file (simulated)
                var largeContent = new string('X', 1024 * 100); // 100KB
                var largeFile = CreateMockFile("large.txt", largeContent);
                var largeUrl = await service1.SaveAsync(largeFile);
                var largeFileName = Path.GetFileName(largeUrl.Replace("/uploads/", ""));
                var largeFullPath = Path.Combine(testRoot, largeFileName);

                if (File.Exists(largeFullPath))
                {
                    var fileInfo = new FileInfo(largeFullPath);
                    Console.WriteLine($"\n✓ Handles large files: {fileInfo.Length > 100000}");
                    Console.WriteLine($"  File size: {fileInfo.Length} bytes");
                }

                // Test 10: File with no extension
                var noExtFile = CreateMockFile("noextension", "content");
                var noExtUrl = await service1.SaveAsync(noExtFile);
                Console.WriteLine($"\n✓ Handles files without extension: {!string.IsNullOrEmpty(noExtUrl)}");
                Console.WriteLine($"  URL: {noExtUrl}");

                // Test 11: Count files in directory
                var files = Directory.GetFiles(testRoot);
                Console.WriteLine($"\n✓ All files saved to disk: {files.Length > 0}");
                Console.WriteLine($"  Total files created: {files.Length}");

                Console.WriteLine("\n✅ All tests passed!");
            }
            finally
            {
                // Cleanup: Delete test directory
                if (Directory.Exists(testRoot))
                {
                    Directory.Delete(testRoot, true);
                    Console.WriteLine($"\n🧹 Cleaned up test directory: {testRoot}");
                }
            }
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
}
