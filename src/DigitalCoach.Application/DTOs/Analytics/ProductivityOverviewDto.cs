namespace DigitalCoach.Application.DTOs.Analytics;

public sealed record ProductivityOverviewDto(
    int UserId,
    DateOnly Date,
    int Energy,
    int Mood,
    int Stress,
    int PhysicalState,
    int TasksCount,
    int CompletedTasks,
    int HabitLogs,
    int CompletedHabits);
