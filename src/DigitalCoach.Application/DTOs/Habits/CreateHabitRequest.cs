namespace DigitalCoach.Application.DTOs.Habits;

public sealed record CreateHabitRequest(
    string Name,
    string? Description,
    string Type,
    int? Frequency,
    string? DaysOfWeek,
    int Difficulty,
    DateOnly StartDate,
    bool IsActive);
