namespace Summit.Models
{
    public class Goal
    {
        public int Id { get; set; }
        public int HabitId { get; set; }
        public int TargetPerPeriod { get; set; } = 5; // e.g., 5 per 'week'
        public string PeriodType { get; set; } = "week"; // week or month

        public Habit Habit { get; set; } = default!;
    }
}
