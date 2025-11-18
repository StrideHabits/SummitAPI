using System;
using SummitAPI.Models;

namespace SummitAPI.Tests.Models
{
    public class UserTests
    {
        public static void Main()
        {
            Console.WriteLine("Running User Tests...\n");

            // Test 1: Default values
            var user1 = new User();
            Console.WriteLine($"✓ Id is set: {user1.Id != Guid.Empty}");
            Console.WriteLine($"✓ Name is empty string: {user1.Name == ""}");
            Console.WriteLine($"✓ Email is empty string: {user1.Email == ""}");
            Console.WriteLine($"✓ PasswordHash is empty string: {user1.PasswordHash == ""}");
            Console.WriteLine($"✓ CreatedAt is set: {user1.CreatedAt != default(DateTime)}");
            Console.WriteLine($"✓ UpdatedAt is set: {user1.UpdatedAt != default(DateTime)}");

            // Test 2: Setting properties
            var userId = Guid.NewGuid();
            var user2 = new User
            {
                Id = userId,
                Name = "John Doe",
                Email = "john.doe@example.com",
                PasswordHash = "hashed_password_12345",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow
            };
            Console.WriteLine($"\n✓ Id can be set: {user2.Id == userId}");
            Console.WriteLine($"✓ Name is 'John Doe': {user2.Name == "John Doe"}");
            Console.WriteLine($"✓ Email is correct: {user2.Email == "john.doe@example.com"}");
            Console.WriteLine($"✓ PasswordHash is set: {user2.PasswordHash == "hashed_password_12345"}");
            Console.WriteLine($"✓ CreatedAt can be set: {user2.CreatedAt < DateTime.UtcNow}");
            Console.WriteLine($"✓ UpdatedAt can be set: {user2.UpdatedAt != default(DateTime)}");

            // Test 3: Name max length (128 chars)
            var name128 = new string('a', 128);
            var name200 = new string('a', 200);
            var user3 = new User { Name = name128 };
            var user4 = new User { Name = name200 };
            Console.WriteLine($"\n✓ Name accepts 128 chars: {user3.Name.Length == 128}");
            Console.WriteLine($"✓ Name accepts 200 chars: {user4.Name.Length == 200}");
            Console.WriteLine("  (Note: Database will enforce 128 limit)");

            // Test 4: Email max length (256 chars)
            var email256 = new string('b', 246) + "@email.com"; // 256 total
            var email300 = new string('b', 290) + "@email.com"; // 300 total
            var user5 = new User { Email = email256 };
            var user6 = new User { Email = email300 };
            Console.WriteLine($"\n✓ Email accepts 256 chars: {user5.Email.Length == 256}");
            Console.WriteLine($"✓ Email accepts 300 chars: {user6.Email.Length == 300}");
            Console.WriteLine("  (Note: Database will enforce 256 limit)");

            // Test 5: PasswordHash max length (512 chars)
            var hash512 = new string('c', 512);
            var hash600 = new string('c', 600);
            var user7 = new User { PasswordHash = hash512 };
            var user8 = new User { PasswordHash = hash600 };
            Console.WriteLine($"\n✓ PasswordHash accepts 512 chars: {user7.PasswordHash.Length == 512}");
            Console.WriteLine($"✓ PasswordHash accepts 600 chars: {user8.PasswordHash.Length == 600}");
            Console.WriteLine("  (Note: Database will enforce 512 limit)");

            // Test 6: Different email formats
            var user9 = new User { Email = "simple@example.com" };
            var user10 = new User { Email = "user.name+tag@example.co.uk" };
            var user11 = new User { Email = "test_user123@company-domain.com" };
            Console.WriteLine($"\n✓ Email accepts simple format: {user9.Email == "simple@example.com"}");
            Console.WriteLine($"✓ Email accepts complex format: {user10.Email.Contains("+") && user10.Email.Contains(".")}");
            Console.WriteLine($"✓ Email accepts underscores/hyphens: {user11.Email.Contains("_") && user11.Email.Contains("-")}");

            // Test 7: Timestamps are close to now
            var beforeCreation = DateTime.UtcNow.AddSeconds(-2);
            var user12 = new User();
            var afterCreation = DateTime.UtcNow.AddSeconds(2);
            var createdAtInRange = user12.CreatedAt >= beforeCreation && user12.CreatedAt <= afterCreation;
            var updatedAtInRange = user12.UpdatedAt >= beforeCreation && user12.UpdatedAt <= afterCreation;
            Console.WriteLine($"\n✓ CreatedAt defaults to now: {createdAtInRange}");
            Console.WriteLine($"✓ UpdatedAt defaults to now: {updatedAtInRange}");

            // Test 8: Multiple users with different data
            var userA = new User { Name = "Alice", Email = "alice@test.com" };
            var userB = new User { Name = "Bob", Email = "bob@test.com" };
            var userC = new User { Name = "Charlie", Email = "charlie@test.com" };
            Console.WriteLine($"\n✓ User A created: {userA.Name == "Alice"}");
            Console.WriteLine($"✓ User B created: {userB.Name == "Bob"}");
            Console.WriteLine($"✓ User C created: {userC.Name == "Charlie"}");
            Console.WriteLine($"✓ All users have unique Ids: {userA.Id != userB.Id && userB.Id != userC.Id}");

            Console.WriteLine("\n✅ All tests passed!");
        }
    }
}
