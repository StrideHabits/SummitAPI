namespace Summit.DTOs
{
    public record HabitCreateDto(string Title, string Frequency, int? TagId);
    public record HabitUpdateDto(string Title, string Frequency, int? TagId);

    public record HabitResponseDto(
        int Id, string Title, string Frequency, DateTime CreatedAt,
        int? TagId, int CurrentStreak, int LongestStreak, double CompletionRate
    );
}
