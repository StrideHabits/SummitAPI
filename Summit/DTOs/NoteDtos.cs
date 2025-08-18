namespace Summit.DTOs
{
    public record NoteCreateDto(int CompletionId, string Content);
    public record NoteResponseDto(int Id, int CompletionId, string Content, DateTime CreatedAt);
}
