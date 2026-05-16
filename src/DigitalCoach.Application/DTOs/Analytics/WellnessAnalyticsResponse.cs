namespace DigitalCoach.Application.DTOs.Analytics;

public sealed record WellnessAnalyticsResponse(
    IReadOnlyList<DailyMetricResponse> MoodTrend,
    IReadOnlyList<DailyMetricResponse> StressTrend,
    IReadOnlyList<DailyMetricResponse> EnergyTrend,
    IReadOnlyList<DailyMetricResponse> SleepTrend,
    IReadOnlyList<DailyMetricResponse> PhysicalStateTrend,
    string BurnoutRiskLevel);
