namespace DigitalCoach.Application.DTOs.Analytics;

public sealed record TaskAnalyticsResponse(
    int TotalTasks,
    int CompletedTasks,
    int CancelledTasks,
    int OverdueTasks,
    int PlannedTasks,
    decimal CompletionRate,
    decimal AverageCompletionTime,
    TaskSummaryResponse? MostRescheduledTask,
    IReadOnlyList<DailyCountResponse> CompletionTrend);
