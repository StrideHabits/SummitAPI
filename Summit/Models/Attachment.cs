namespace Summit.Models
{
    public class Attachment
    {
        public int Id { get; set; }
        public int CompletionId { get; set; }
        public string FileUrl { get; set; } = string.Empty;

        public Completion Completion { get; set; } = default!;
    }
}
