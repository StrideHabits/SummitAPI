namespace SummitAPI.Dtos
{
    public record CheckInCreateDto(Guid HabitId, DateTime? CompletedAt, string? DayKey);
    public record CheckInDto(Guid Id, Guid HabitId, DateTime CompletedAt, string DayKey);
}
