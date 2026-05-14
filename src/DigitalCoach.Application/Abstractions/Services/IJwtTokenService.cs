using DigitalCoach.Application.DTOs.Auth;
using DigitalCoach.Domain.Entities;

namespace DigitalCoach.Application.Abstractions.Services;

public interface IJwtTokenService
{
    AuthResponse CreateToken(UserProfile user);
}
