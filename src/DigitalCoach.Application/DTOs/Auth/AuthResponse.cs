namespace DigitalCoach.Application.DTOs.Auth;

public sealed record AuthResponse(int UserId, string Email, string AccessToken, DateTime ExpiresAt);
