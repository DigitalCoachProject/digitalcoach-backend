namespace DigitalCoach.Application.DTOs.Tasks;

public sealed record RescheduleTaskRequest(
    DateOnly NewPlannedDate,
    string? Reason);
