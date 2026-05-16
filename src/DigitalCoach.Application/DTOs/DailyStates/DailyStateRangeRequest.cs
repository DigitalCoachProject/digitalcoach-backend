namespace DigitalCoach.Application.DTOs.DailyStates;

public sealed record DailyStateRangeRequest(DateOnly? From, DateOnly? To);
