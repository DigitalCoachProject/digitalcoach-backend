namespace DigitalCoach.Application.DTOs.Habits;

public sealed record HabitLogResponse(
    int Id,
    int HabitId,
    DateOnly Date,
    string Status,
    string? Reason,
    string? Comment);
