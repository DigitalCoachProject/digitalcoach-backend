namespace DigitalCoach.Application.DTOs.Analytics;

public sealed record ProductivityTrendResponse(
    DateOnly Date,
    decimal ProductivityScore,
    int TasksCount,
    int CompletedTasks,
    int HabitLogs,
    int CompletedHabits);
