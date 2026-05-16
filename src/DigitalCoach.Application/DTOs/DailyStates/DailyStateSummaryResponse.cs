namespace DigitalCoach.Application.DTOs.DailyStates;

public sealed record DailyStateSummaryResponse(
    decimal? AverageMood,
    decimal? AverageStress,
    decimal? AverageEnergy,
    decimal? AverageSleepQuality,
    decimal? AveragePhysicalState,
    decimal? AverageScreenTime,
    decimal TotalActivityMinutes,
    int TotalRestDays,
    int TotalEntries);
