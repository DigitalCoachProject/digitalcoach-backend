namespace DigitalCoach.Application.DTOs.Habits;

public sealed record HabitLogFilterRequest(DateOnly? From, DateOnly? To);
