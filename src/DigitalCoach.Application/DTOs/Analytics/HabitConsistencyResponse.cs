namespace DigitalCoach.Application.DTOs.Analytics;

public sealed record HabitConsistencyResponse(
    int HabitId,
    string HabitName,
    decimal CompletionRate);
