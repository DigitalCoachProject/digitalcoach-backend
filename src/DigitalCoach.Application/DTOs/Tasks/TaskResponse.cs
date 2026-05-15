namespace DigitalCoach.Application.DTOs.Tasks;

public sealed record TaskResponse(
    int Id,
    int UserId,
    string Name,
    string? Description,
    DateTime CreatedAt,
    DateOnly PlannedDate,
    DateOnly? Deadline,
    int Priority,
    int Difficulty,
    string Status,
    DateTime? CompletedAt,
    int RescheduleCount,
    DateTime UpdatedAt);
