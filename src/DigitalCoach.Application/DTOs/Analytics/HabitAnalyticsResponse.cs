namespace DigitalCoach.Application.DTOs.Analytics;

public sealed record HabitAnalyticsResponse(
    int TotalHabits,
    int ActiveHabits,
    int CompletedLogs,
    int FailedLogs,
    int SkippedLogs,
    decimal CompletionRate,
    HabitConsistencyResponse? MostConsistentHabit,
    HabitConsistencyResponse? LeastConsistentHabit,
    IReadOnlyList<HabitStreakResponse> CurrentStreaks,
    IReadOnlyList<HabitStreakResponse> LongestStreaks);
