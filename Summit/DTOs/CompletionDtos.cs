namespace Summit.DTOs
{
    public record CompletionCreateDto(int HabitId, DateTime? Date);
    public record CompletionResponseDto(int Id, int HabitId, DateTime CompletedOn);
}
