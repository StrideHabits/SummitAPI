namespace Summit.DTOs
{
    public record TagCreateDto(string Name);
    public record TagUpdateDto(string Name);
    public record TagResponseDto(int Id, string Name);
}
