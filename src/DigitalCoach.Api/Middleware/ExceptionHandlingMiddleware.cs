using System.Text.Json;
using DigitalCoach.Api.Common;

namespace DigitalCoach.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IWebHostEnvironment environment)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception while processing request {Method} {Path}.", context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var response = environment.IsDevelopment()
                ? ApiResponse<object>.Failure(
                    exception.Message,
                    new Dictionary<string, string[]>
                    {
                        ["exception"] = [exception.GetType().FullName ?? exception.GetType().Name],
                        ["innerException"] = exception.InnerException is null
                            ? []
                            : [exception.InnerException.Message],
                        ["stackTrace"] = exception.StackTrace is null ? [] : [exception.StackTrace]
                    })
                : ApiResponse<object>.Failure("An unexpected server error occurred.");

            await JsonSerializer.SerializeAsync(context.Response.Body, response, JsonOptions, context.RequestAborted);
        }
    }
}
