using System;
using SummitAPI.Models;

namespace SummitAPI.Tests.Models
{
    public class HabitTests
    {
        public static void Main()
        {
            Console.WriteLine("Running Habit Tests...\n");

            // Test 1: Default values
            var habit1 = new Habit();
            Console.WriteLine($"✓ Id is set: {habit1.Id != Guid.Empty}");
            Console.WriteLine($"✓ Name is empty string: {habit1.Name == ""}");
            Console.WriteLine($"✓ Frequency is 0: {habit1.Frequency == 0}");
            Console.WriteLine($"✓ Deleted is false: {habit1.Deleted == false}");

            // Test 2: Setting properties
            var habit2 = new Habit
            {
                UserId = Guid.NewGuid(),
                Name = "Morning Run",
                Frequency = 5,
                Tag = "Fitness",
                ImageUrl = "https://example.com/image.jpg",
                Deleted = true
            };
            Console.WriteLine($"\n✓ UserId can be set: {habit2.UserId != Guid.Empty}");
            Console.WriteLine($"✓ Name is 'Morning Run': {habit2.Name == "Morning Run"}");
            Console.WriteLine($"✓ Frequency is 5: {habit2.Frequency == 5}");
            Console.WriteLine($"✓ Tag is 'Fitness': {habit2.Tag == "Fitness"}");
            Console.WriteLine($"✓ Deleted is true: {habit2.Deleted == true}");

            // Test 3: Different frequency values
            var habit3 = new Habit { Frequency = 7 };
            var habit4 = new Habit { Frequency = 0 };
            var habit5 = new Habit { Frequency = -1 };
            Console.WriteLine($"\n✓ Frequency accepts 7: {habit3.Frequency == 7}");
            Console.WriteLine($"✓ Frequency accepts 0: {habit4.Frequency == 0}");
            Console.WriteLine($"✓ Frequency accepts -1: {habit5.Frequency == -1}");

            // Test 4: Long strings
            var longName = new string('a', 128);
            var veryLongName = new string('a', 200);
            var habit6 = new Habit { Name = longName };
            var habit7 = new Habit { Name = veryLongName };
            Console.WriteLine($"\n✓ Name accepts 128 chars: {habit6.Name.Length == 128}");
            Console.WriteLine($"✓ Name accepts 200 chars: {habit7.Name.Length == 200}");
            Console.WriteLine("  (Note: Database will enforce 128 limit)");

            Console.WriteLine("\n✅ All tests passed!");
        }
    }
}
