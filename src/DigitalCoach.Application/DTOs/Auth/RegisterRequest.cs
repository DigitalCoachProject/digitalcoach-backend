namespace DigitalCoach.Application.DTOs.Auth;

public sealed record RegisterRequest(
    string Email,
    string Password,
    DateOnly BirthDate,
    decimal Height,
    decimal Weight,
    string? Gender);
