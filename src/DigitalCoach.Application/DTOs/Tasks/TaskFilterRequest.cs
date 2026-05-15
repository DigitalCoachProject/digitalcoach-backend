namespace DigitalCoach.Application.DTOs.Tasks;

public sealed record TaskFilterRequest(
    string? Status,
    int? Priority,
    DateOnly? From,
    DateOnly? To,
    bool? Overdue,
    string? SortBy,
    bool SortDescending = false);
