namespace DigitalCoach.Application.DTOs.Analytics;

public sealed record DashboardAnalyticsResponse(
    int TotalHabits,
    int ActiveHabits,
    int CompletedHabitLogs,
    decimal HabitCompletionRate,
    int TotalTasks,
    int CompletedTasks,
    int CancelledTasks,
    int OverdueTasks,
    decimal TaskCompletionRate,
    decimal AverageMood,
    decimal AverageStress,
    decimal AverageEnergy,
    decimal AverageSleepQuality,
    decimal AveragePhysicalState,
    decimal TotalActivityMinutes,
    decimal AverageScreenTime,
    decimal ProductivityScore);
