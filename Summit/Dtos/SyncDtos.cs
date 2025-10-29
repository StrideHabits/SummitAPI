namespace SummitAPI.Dtos
{
    public record CheckInPushItem(
        string RequestId,
        Guid Id,
        Guid HabitId,
        string DayKey,
        DateTime CompletedAt,
        bool Deleted,
        string? BaseVersion // base64 RowVersion from last pull/push; null for creates
    );

    public record PushResult(
        Guid Id,
        DateTime UpdatedAt,
        string RowVersion,       // base64
        string Status,           // "applied" | "conflict"
        string? ConflictReason
    );

    public record CheckInChange(
        Guid Id,
        Guid HabitId,
        string DayKey,
        DateTime CompletedAt,
        bool Deleted,
        DateTime UpdatedAt,
        string RowVersion        // base64
    );

    public record ChangesPage(
        List<CheckInChange> Items,
        bool HasMore,
        DateTime? NextSince
    );
}
