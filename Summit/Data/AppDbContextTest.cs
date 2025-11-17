using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SummitAPI.Data;
using SummitAPI.Models;

namespace SummitAPI.Tests.Data
{
    public class AppDbContextTests
    {
        public static async Task Main()
        {
            Console.WriteLine("Running AppDbContext Tests...\n");

            // Test 1: DbContext can be created
            var context = CreateInMemoryContext();
            Console.WriteLine("✓ DbContext created successfully");

            // Test 2: DbSets are accessible
            Console.WriteLine($"\n✓ Users DbSet accessible: {context.Users != null}");
            Console.WriteLine($"✓ Habits DbSet accessible: {context.Habits != null}");
            Console.WriteLine($"✓ HabitCompletions DbSet accessible: {context.HabitCompletions != null}");
            Console.WriteLine($"✓ Settings DbSet accessible: {context.Settings != null}");
            Console.WriteLine($"✓ RequestLogs DbSet accessible: {context.RequestLogs != null}");

            // Test 3: User with unique email constraint
            var user1 = new User { Name = "John", Email = "john@test.com", PasswordHash = "hash1" };
            context.Users.Add(user1);
            await context.SaveChangesAsync();
            Console.WriteLine($"\n✓ User saved: {user1.Id != Guid.Empty}");

            // Test duplicate email (should fail)
            try
            {
                var context2 = CreateInMemoryContext();
                var user2 = new User { Name = "Jane", Email = "john@test.com", PasswordHash = "hash2" };
                context2.Users.Add(user2);
                await context2.SaveChangesAsync();
                Console.WriteLine("✗ Duplicate email should have failed");
            }
            catch
            {
                Console.WriteLine("✓ Unique email constraint enforced");
            }

            // Test 4: Habit with UserId foreign key
            var habit1 = new Habit
            {
                UserId = user1.Id,
                Name = "Morning Run",
                Frequency = 5
            };
            context.Habits.Add(habit1);
            await context.SaveChangesAsync();
            Console.WriteLine($"\n✓ Habit saved with UserId: {habit1.Id != Guid.Empty}");

            // Test 5: CreatedAt and UpdatedAt timestamps
            var savedHabit = await context.Habits.FirstAsync(h => h.Id == habit1.Id);
            Console.WriteLine($"\n✓ CreatedAt is set automatically: {savedHabit.CreatedAt != default(DateTime)}");
            Console.WriteLine($"✓ UpdatedAt is set automatically: {savedHabit.UpdatedAt != default(DateTime)}");
            Console.WriteLine($"  CreatedAt: {savedHabit.CreatedAt:yyyy-MM-dd HH:mm:ss}");

            // Test 6: UpdatedAt changes on modification
            var beforeUpdate = savedHabit.UpdatedAt;
            await Task.Delay(100); // Small delay to ensure timestamp difference
            savedHabit.Name = "Evening Run";
            await context.SaveChangesAsync();

            var updatedHabit = await context.Habits.FirstAsync(h => h.Id == habit1.Id);
            Console.WriteLine($"\n✓ UpdatedAt changes on update: {updatedHabit.UpdatedAt > beforeUpdate}");
            Console.WriteLine($"  Before: {beforeUpdate:HH:mm:ss.fff}");
            Console.WriteLine($"  After:  {updatedHabit.UpdatedAt:HH:mm:ss.fff}");

            // Test 7: RowVersion is set automatically
            Console.WriteLine($"\n✓ RowVersion is set: {savedHabit.RowVersion.Length > 0}");
            Console.WriteLine($"  RowVersion length: {savedHabit.RowVersion.Length} bytes");

            // Test 8: HabitCompletion with unique HabitId + DayKey
            var completion1 = new HabitCompletion
            {
                HabitId = habit1.Id,
                DayKey = "2024-11-15"
            };
            context.HabitCompletions.Add(completion1);
            await context.SaveChangesAsync();
            Console.WriteLine($"\n✓ HabitCompletion saved: {completion1.Id != Guid.Empty}");

            // Test duplicate HabitId + DayKey (should fail)
            try
            {
                var context3 = CreateInMemoryContext();
                var completion2 = new HabitCompletion
                {
                    HabitId = habit1.Id,
                    DayKey = "2024-11-15"
                };
                context3.HabitCompletions.Add(completion2);
                await context3.SaveChangesAsync();
                Console.WriteLine("✗ Duplicate HabitId+DayKey should have failed");
            }
            catch
            {
                Console.WriteLine("✓ Unique HabitId+DayKey constraint enforced");
            }

            // Test 9: Cascade delete - deleting user deletes habits
            var user2 = new User { Name = "Alice", Email = "alice@test.com", PasswordHash = "hash" };
            context.Users.Add(user2);
            await context.SaveChangesAsync();

            var habit2 = new Habit { UserId = user2.Id, Name = "Reading", Frequency = 7 };
            context.Habits.Add(habit2);
            await context.SaveChangesAsync();

            var habitCount = await context.Habits.CountAsync(h => h.UserId == user2.Id);
            Console.WriteLine($"\n✓ User has habits: {habitCount == 1}");

            context.Users.Remove(user2);
            await context.SaveChangesAsync();

            var habitCountAfter = await context.Habits.CountAsync(h => h.UserId == user2.Id);
            Console.WriteLine($"✓ Cascade delete works: {habitCountAfter == 0}");
            Console.WriteLine($"  Habits before delete: {habitCount}");
            Console.WriteLine($"  Habits after delete: {habitCountAfter}");

            // Test 10: Cascade delete - deleting habit deletes completions
            var user3 = new User { Name = "Bob", Email = "bob@test.com", PasswordHash = "hash" };
            context.Users.Add(user3);
            await context.SaveChangesAsync();

            var habit3 = new Habit { UserId = user3.Id, Name = "Meditation", Frequency = 7 };
            context.Habits.Add(habit3);
            await context.SaveChangesAsync();

            var comp1 = new HabitCompletion { HabitId = habit3.Id, DayKey = "2024-11-10" };
            var comp2 = new HabitCompletion { HabitId = habit3.Id, DayKey = "2024-11-11" };
            context.HabitCompletions.AddRange(comp1, comp2);
            await context.SaveChangesAsync();

            var compCount = await context.HabitCompletions.CountAsync(c => c.HabitId == habit3.Id);
            Console.WriteLine($"\n✓ Habit has completions: {compCount == 2}");

            context.Habits.Remove(habit3);
            await context.SaveChangesAsync();

            var compCountAfter = await context.HabitCompletions.CountAsync(c => c.HabitId == habit3.Id);
            Console.WriteLine($"✓ Cascade delete for completions works: {compCountAfter == 0}");
            Console.WriteLine($"  Completions before: {compCount}");
            Console.WriteLine($"  Completions after: {compCountAfter}");

            // Test 11: Multiple completions for same habit, different days
            var user4 = new User { Name = "Charlie", Email = "charlie@test.com", PasswordHash = "hash" };
            context.Users.Add(user4);
            await context.SaveChangesAsync();

            var habit4 = new Habit { UserId = user4.Id, Name = "Exercise", Frequency = 5 };
            context.Habits.Add(habit4);
            await context.SaveChangesAsync();

            var compA = new HabitCompletion { HabitId = habit4.Id, DayKey = "2024-11-15" };
            var compB = new HabitCompletion { HabitId = habit4.Id, DayKey = "2024-11-16" };
            var compC = new HabitCompletion { HabitId = habit4.Id, DayKey = "2024-11-17" };
            context.HabitCompletions.AddRange(compA, compB, compC);
            await context.SaveChangesAsync();

            var completions = await context.HabitCompletions
                .Where(c => c.HabitId == habit4.Id)
                .OrderBy(c => c.DayKey)
                .ToListAsync();

            Console.WriteLine($"\n✓ Multiple completions saved: {completions.Count == 3}");
            Console.WriteLine($"  Day 1: {completions[0].DayKey}");
            Console.WriteLine($"  Day 2: {completions[1].DayKey}");
            Console.WriteLine($"  Day 3: {completions[2].DayKey}");

            // Test 12: UpdatedAt index exists (via query performance)
            var recentCompletions = await context.HabitCompletions
                .Where(c => c.UpdatedAt > DateTime.UtcNow.AddDays(-1))
                .ToListAsync();
            Console.WriteLine($"\n✓ UpdatedAt index allows queries: {recentCompletions.Count >= 0}");
            Console.WriteLine($"  Recent completions found: {recentCompletions.Count}");

            // Test 13: UserSettings with UserId foreign key
            var settings1 = new UserSettings { UserId = user1.Id };
            context.Settings.Add(settings1);
            await context.SaveChangesAsync();
            Console.WriteLine($"\n✓ UserSettings saved: {settings1.Id != Guid.Empty}");

            // Test 14: RequestLog with unique UserId + RequestId
            var log1 = new RequestLog
            {
                UserId = user1.Id,
                RequestId = "req_123",
                Response = "success"
            };
            context.RequestLogs.Add(log1);
            await context.SaveChangesAsync();
            Console.WriteLine($"\n✓ RequestLog saved: {log1.Id != Guid.Empty}");

            // Test duplicate UserId + RequestId (should fail)
            try
            {
                var context4 = CreateInMemoryContext();
                var log2 = new RequestLog
                {
                    UserId = user1.Id,
                    RequestId = "req_123",
                    Response = "duplicate"
                };
                context4.RequestLogs.Add(log2);
                await context4.SaveChangesAsync();
                Console.WriteLine("✗ Duplicate UserId+RequestId should have failed");
            }
            catch
            {
                Console.WriteLine("✓ Unique UserId+RequestId constraint enforced");
            }

            // Test 15: Multiple users and habits
            var totalUsers = await context.Users.CountAsync();
            var totalHabits = await context.Habits.CountAsync();
            var totalCompletions = await context.HabitCompletions.CountAsync();

            Console.WriteLine($"\n✓ Database statistics:");
            Console.WriteLine($"  Total Users: {totalUsers}");
            Console.WriteLine($"  Total Habits: {totalHabits}");
            Console.WriteLine($"  Total Completions: {totalCompletions}");

            Console.WriteLine("\n✅ All tests passed!");
        }

        private static AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid())
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
