namespace DigitalCoach.Domain.Constants;

public static class NotificationTypes
{
    public const string Habit = "habit";
    public const string Task = "task";
    public const string Wellness = "wellness";
    public const string Recommendation = "recommendation";
    public const string Burnout = "burnout";
    public const string Reminder = "reminder";
    public const string System = "system";

    public static readonly string[] All =
    [
        Habit,
        Task,
        Wellness,
        Recommendation,
        Burnout,
        Reminder,
        System
    ];
}
