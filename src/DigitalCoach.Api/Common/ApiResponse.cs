namespace DigitalCoach.Api.Common;

public sealed record ApiResponse<T>(
    bool Succeeded,
    T? Data,
    string? Message,
    IReadOnlyDictionary<string, string[]>? Errors = null)
{
    public static ApiResponse<T> Success(T data, string? message = null)
    {
        return new ApiResponse<T>(true, data, message);
    }

    public static ApiResponse<T> Failure(string message, IReadOnlyDictionary<string, string[]>? errors = null)
    {
        return new ApiResponse<T>(false, default, message, errors);
    }
}
