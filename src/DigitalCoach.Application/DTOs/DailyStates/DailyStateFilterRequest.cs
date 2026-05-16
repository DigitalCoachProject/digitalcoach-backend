namespace DigitalCoach.Application.DTOs.DailyStates;

public sealed record DailyStateFilterRequest(
    DateOnly? From,
    DateOnly? To,
    int? Mood,
    int? Stress,
    int? Energy,
    int? PhysicalState,
    bool? HasIllness,
    bool? HasPainOrInjury,
    string? ActivityType);
