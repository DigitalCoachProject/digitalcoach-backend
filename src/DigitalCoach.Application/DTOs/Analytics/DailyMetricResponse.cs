namespace DigitalCoach.Application.DTOs.Analytics;

public sealed record DailyMetricResponse(DateOnly Date, decimal Value);
