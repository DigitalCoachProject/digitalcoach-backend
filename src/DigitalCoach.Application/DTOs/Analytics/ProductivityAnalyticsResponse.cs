namespace DigitalCoach.Application.DTOs.Analytics;

public sealed record ProductivityAnalyticsResponse(
    IReadOnlyList<DailyCountResponse> CompletedTasksByDay,
    IReadOnlyList<DailyCountResponse> CompletedHabitsByDay,
    IReadOnlyList<ProductivityTrendResponse> ProductivityTrend,
    string? MostProductiveDayType,
    int OverdueTasksCount,
    decimal AverageTasksPerDay);
