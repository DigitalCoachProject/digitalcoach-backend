namespace DigitalCoach.Application.DTOs.Habits;

public sealed record HabitResponse(
    int Id,
    int UserId,
    string Name,
    string? Description,
    string Type,
    int? Frequency,
    string? DaysOfWeek,
    int Difficulty,
    DateOnly StartDate,
    bool IsActive,
    DateTime UpdatedAt);
