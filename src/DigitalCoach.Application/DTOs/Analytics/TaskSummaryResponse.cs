namespace DigitalCoach.Application.DTOs.Analytics;

public sealed record TaskSummaryResponse(
    int TaskId,
    string TaskName,
    int Count);
