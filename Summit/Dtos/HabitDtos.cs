namespace SummitAPI.Dtos
{
    public record HabitCreateDto(string Name, int Frequency, string? Tag, string? ImageUrl);
    public record HabitDto(Guid Id, string Name, int Frequency, string? Tag, string? ImageUrl,
                           DateTime CreatedAt, DateTime UpdatedAt);
}
