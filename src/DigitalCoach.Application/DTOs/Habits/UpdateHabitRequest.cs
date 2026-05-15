namespace DigitalCoach.Application.DTOs.Habits;

public sealed record UpdateHabitRequest(
    string Name,
    string? Description,
    string Type,
    int? Frequency,
    string? DaysOfWeek,
    int Difficulty,
    DateOnly StartDate,
    bool IsActive);
