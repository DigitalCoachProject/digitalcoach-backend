namespace DigitalCoach.Application.DTOs.Recommendations;

public sealed record RecommendationResponse(
    int Id,
    string Type,
    string Title,
    string Message,
    int Priority,
    bool IsRead,
    DateTime CreatedAt,
    DateTime? ExpiresAt);
