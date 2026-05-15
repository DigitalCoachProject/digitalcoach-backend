namespace DigitalCoach.Application.DTOs.Habits;

public sealed record CreateHabitLogRequest(
    DateOnly Date,
    string Status,
    string? Reason,
    string? Comment);
