namespace DigitalCoach.Application.DTOs.Analytics;

public sealed record AnalyticsRangeRequest(DateOnly? From, DateOnly? To);
