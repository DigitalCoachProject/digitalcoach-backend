namespace DigitalCoach.Application.Common;

public enum ErrorType
{
    None,
    Validation,
    Conflict,
    Forbidden,
    Unauthorized,
    NotFound
}

public sealed record Result(bool Succeeded, string? Error = null, ErrorType ErrorType = ErrorType.None)
{
    public static Result Success() => new(true);
    public static Result Failure(string error, ErrorType errorType = ErrorType.Validation) => new(false, error, errorType);
}

public sealed record Result<T>(bool Succeeded, T? Value = default, string? Error = null, ErrorType ErrorType = ErrorType.None)
{
    public static Result<T> Success(T value) => new(true, value);
    public static Result<T> Failure(string error, ErrorType errorType = ErrorType.Validation) => new(false, default, error, errorType);
}
