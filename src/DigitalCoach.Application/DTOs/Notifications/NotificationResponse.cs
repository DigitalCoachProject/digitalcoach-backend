namespace DigitalCoach.Application.DTOs.Notifications;

public sealed record NotificationResponse(
    int Id,
    string Type,
    string Title,
    string Message,
    int Priority,
    bool IsRead,
    DateTime CreatedAt,
    DateTime? ScheduledFor,
    DateTime? ReadAt,
    DateTime? ExpiresAt);
