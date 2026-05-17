using System.Diagnostics;
using System.Security.Claims;

namespace DigitalCoach.Api.Middleware;

public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        await next(context);

        stopwatch.Stop();
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        var correlationId = context.TraceIdentifier;
        var statusCode = context.Response.StatusCode;

        var message = "HTTP {Method} {Path} -> {StatusCode} in {ElapsedMilliseconds}ms (user: {UserId}, correlationId: {CorrelationId})";
        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(message, context.Request.Method, context.Request.Path, statusCode, elapsedMilliseconds, userId, correlationId);
        }
        else if (statusCode >= StatusCodes.Status400BadRequest)
        {
            logger.LogWarning(message, context.Request.Method, context.Request.Path, statusCode, elapsedMilliseconds, userId, correlationId);
        }
        else
        {
            logger.LogInformation(message, context.Request.Method, context.Request.Path, statusCode, elapsedMilliseconds, userId, correlationId);
        }
    }
}
