using DigitalCoach.Application.Common;
using DigitalCoach.Application.DTOs.Auth;

namespace DigitalCoach.Application.Abstractions.Services;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
