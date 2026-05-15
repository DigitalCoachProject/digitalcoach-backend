namespace DigitalCoach.Application.DTOs.Habits;

public sealed record HabitFilterRequest(
    string? Type,
    bool? IsActive,
    DateOnly? StartDateFrom,
    DateOnly? StartDateTo);
