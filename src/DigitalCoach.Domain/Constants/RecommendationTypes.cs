namespace DigitalCoach.Domain.Constants;

public static class RecommendationTypes
{
    public const string Productivity = "productivity";
    public const string Wellness = "wellness";
    public const string Habit = "habit";
    public const string Task = "task";
    public const string Burnout = "burnout";
    public const string Sleep = "sleep";
    public const string Motivation = "motivation";

    public static readonly string[] All =
    [
        Productivity,
        Wellness,
        Habit,
        Task,
        Burnout,
        Sleep,
        Motivation
    ];
}
