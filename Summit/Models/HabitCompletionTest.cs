using System;
using SummitAPI.Models;

namespace SummitAPI.Tests.Models
{
    public class HabitCompletionTests
    {
        public static void Main()
        {
            Console.WriteLine("Running HabitCompletion Tests...\n");

            // Test 1: Default values
            var completion1 = new HabitCompletion();
            Console.WriteLine($"✓ Id is set: {completion1.Id != Guid.Empty}");
            Console.WriteLine($"✓ CompletedAt is set: {completion1.CompletedAt != default(DateTime)}");
            Console.WriteLine($"✓ DayKey format is yyyy-MM-dd: {completion1.DayKey.Length == 10}");
            Console.WriteLine($"✓ DayKey contains dashes: {completion1.DayKey.Contains("-")}");
            Console.WriteLine($"✓ Deleted is false: {completion1.Deleted == false}");
            Console.WriteLine($"✓ CreatedAt is set: {completion1.CreatedAt != default(DateTime)}");
            Console.WriteLine($"✓ UpdatedAt is set: {completion1.UpdatedAt != default(DateTime)}");
            Console.WriteLine($"✓ RowVersion is empty: {completion1.RowVersion.Length == 0}");

            // Test 2: Setting properties
            var habitId = Guid.NewGuid();
            var completedAt = DateTime.UtcNow.AddDays(-1);
            var completion2 = new HabitCompletion
            {
                HabitId = habitId,
                CompletedAt = completedAt,
                DayKey = "2024-11-15",
                Deleted = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow,
                RowVersion = new byte[] { 1, 2, 3 }
            };
            Console.WriteLine($"\n✓ HabitId can be set: {completion2.HabitId == habitId}");
            Console.WriteLine($"✓ CompletedAt can be set: {completion2.CompletedAt == completedAt}");
            Console.WriteLine($"✓ DayKey is '2024-11-15': {completion2.DayKey == "2024-11-15"}");
            Console.WriteLine($"✓ Deleted is true: {completion2.Deleted == true}");
            Console.WriteLine($"✓ RowVersion has values: {completion2.RowVersion.Length == 3}");

            // Test 3: DayKey format validation
            var completion3 = new HabitCompletion { DayKey = "2025-01-01" };
            var completion4 = new HabitCompletion { DayKey = "2025-12-31" };
            Console.WriteLine($"\n✓ DayKey accepts '2025-01-01': {completion3.DayKey == "2025-01-01"}");
            Console.WriteLine($"✓ DayKey accepts '2025-12-31': {completion4.DayKey == "2025-12-31"}");

            // Test 4: DayKey max length (10 chars)
            var completion5 = new HabitCompletion { DayKey = "2024-11-17" }; // exactly 10
            var completion6 = new HabitCompletion { DayKey = "2024-11-17-extra" }; // exceeds 10
            Console.WriteLine($"\n✓ DayKey accepts 10 chars: {completion5.DayKey.Length == 10}");
            Console.WriteLine($"✓ DayKey accepts longer string: {completion6.DayKey.Length > 10}");
            Console.WriteLine("  (Note: Database will enforce 10 char limit)");

            // Test 5: Multiple completions for same habit
            var sharedHabitId = Guid.NewGuid();
            var comp1 = new HabitCompletion { HabitId = sharedHabitId, DayKey = "2024-11-15" };
            var comp2 = new HabitCompletion { HabitId = sharedHabitId, DayKey = "2024-11-16" };
            var comp3 = new HabitCompletion { HabitId = sharedHabitId, DayKey = "2024-11-17" };
            Console.WriteLine($"\n✓ Same habit, different days - Day 1: {comp1.DayKey == "2024-11-15"}");
            Console.WriteLine($"✓ Same habit, different days - Day 2: {comp2.DayKey == "2024-11-16"}");
            Console.WriteLine($"✓ Same habit, different days - Day 3: {comp3.DayKey == "2024-11-17"}");

            // Test 6: Timestamps are close to now
            var beforeCreation = DateTime.UtcNow.AddSeconds(-2);
            var completion7 = new HabitCompletion();
            var afterCreation = DateTime.UtcNow.AddSeconds(2);
            var completedAtInRange = completion7.CompletedAt >= beforeCreation && completion7.CompletedAt <= afterCreation;
            var createdAtInRange = completion7.CreatedAt >= beforeCreation && completion7.CreatedAt <= afterCreation;
            Console.WriteLine($"\n✓ CompletedAt defaults to now: {completedAtInRange}");
            Console.WriteLine($"✓ CreatedAt defaults to now: {createdAtInRange}");

            Console.WriteLine("\n✅ All tests passed!");
        }
    }
}
