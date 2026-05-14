using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Application.Abstractions.Services;
using DigitalCoach.Application.Common;
using DigitalCoach.Application.DTOs.Auth;
using DigitalCoach.Domain.Entities;

namespace DigitalCoach.Infrastructure.Services;

public sealed class AuthService(
    IUserProfileRepository userProfileRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IAuthService
{
    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existingUser = await userProfileRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (existingUser is not null)
        {
            return Result<AuthResponse>.Failure("User with this email already exists.");
        }

        var user = new UserProfile
        {
            Email = normalizedEmail,
            PasswordHash = passwordHasher.Hash(request.Password),
            BirthDate = request.BirthDate,
            Height = request.Height,
            Weight = request.Weight,
            Gender = request.Gender,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await userProfileRepository.AddAsync(user, cancellationToken);
        await userProfileRepository.SaveChangesAsync(cancellationToken);

        return Result<AuthResponse>.Success(jwtTokenService.CreateToken(user));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userProfileRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result<AuthResponse>.Failure("Invalid email or password.");
        }

        return Result<AuthResponse>.Success(jwtTokenService.CreateToken(user));
    }
}
