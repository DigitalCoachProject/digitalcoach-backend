namespace DigitalCoach.Application.DTOs.Tasks;

public sealed record CreateTaskRequest(
    string Name,
    string? Description,
    DateOnly PlannedDate,
    DateOnly? Deadline,
    int Priority,
    int Difficulty,
    string Status);
