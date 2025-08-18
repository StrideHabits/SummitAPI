namespace Summit.DTOs
{
    public record GoalUpsertDto(int TargetPerPeriod, string PeriodType);
    public record GoalResponseDto(int Id, int HabitId, int TargetPerPeriod, string PeriodType);
}
