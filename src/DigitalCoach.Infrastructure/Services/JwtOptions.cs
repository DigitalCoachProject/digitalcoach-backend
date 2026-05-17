namespace DigitalCoach.Infrastructure.Services;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public int ExpiresMinutes { get; init; } = 60;

    public string SigningKey => string.IsNullOrWhiteSpace(Key) ? SecretKey : Key;
}
