namespace DigitalCoach.Application.DTOs.Analytics;

public sealed record HabitStreakResponse(
    int HabitId,
    string HabitName,
    int Streak);
