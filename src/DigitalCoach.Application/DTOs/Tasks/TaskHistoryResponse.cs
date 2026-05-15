namespace DigitalCoach.Application.DTOs.Tasks;

public sealed record TaskHistoryResponse(
    int Id,
    int TaskId,
    DateTime ChangeDate,
    DateOnly OldDate,
    DateOnly NewDate,
    string? Reason);
