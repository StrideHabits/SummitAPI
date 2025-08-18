namespace Summit.Models
{
    public class Note
    {
        public int Id { get; set; }
        public int CompletionId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Completion Completion { get; set; } = default!;
    }
}
