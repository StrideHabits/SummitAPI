namespace Summit.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;

        public User User { get; set; } = default!;
        public List<Habit> Habits { get; set; } = new();
    }
}
