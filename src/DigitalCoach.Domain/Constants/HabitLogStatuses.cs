namespace DigitalCoach.Domain.Constants;

public static class HabitLogStatuses
{
    public const string Completed = "completed";
    public const string Failed = "failed";
    public const string Skipped = "skipped";

    public static readonly string[] All = [Completed, Failed, Skipped];
}
