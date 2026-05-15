namespace DigitalCoach.Application.DTOs.Auth;

public sealed record CurrentUserResponse(
    int UserId,
    string Email,
    DateOnly BirthDate,
    decimal Height,
    decimal Weight,
    string? Gender,
    DateTime CreatedAt,
    DateTime UpdatedAt);
