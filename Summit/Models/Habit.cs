namespace Summit.Models
{
    public class Habit
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string Title { get; set; } = string.Empty;     // e.g. "Read"
        public string Frequency { get; set; } = "daily";      // daily/weekly/monthly
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? TagId { get; set; }

        public User User { get; set; } = default!;
        public Tag? Tag { get; set; }
        public List<Completion> Completions { get; set; } = new();
        public Goal? Goal { get; set; }
    }
}
