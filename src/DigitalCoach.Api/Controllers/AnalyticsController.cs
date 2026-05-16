using System.Security.Claims;
using DigitalCoach.Api.Common;
using DigitalCoach.Application.Abstractions.Services;
using DigitalCoach.Application.Common;
using DigitalCoach.Application.DTOs.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalCoach.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/analytics")]
public sealed class AnalyticsController(IAnalyticsService analyticsService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard([FromQuery] AnalyticsRangeRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await analyticsService.GetDashboardAsync(userId, request, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<DashboardAnalyticsResponse>.Success(result.Value!))
            : ToErrorResponse(result);
    }

    [HttpGet("productivity")]
    public async Task<IActionResult> GetProductivity([FromQuery] AnalyticsRangeRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await analyticsService.GetProductivityAsync(userId, request, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<ProductivityAnalyticsResponse>.Success(result.Value!))
            : ToErrorResponse(result);
    }

    [HttpGet("wellness")]
    public async Task<IActionResult> GetWellness([FromQuery] AnalyticsRangeRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await analyticsService.GetWellnessAsync(userId, request, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<WellnessAnalyticsResponse>.Success(result.Value!))
            : ToErrorResponse(result);
    }

    [HttpGet("habits")]
    public async Task<IActionResult> GetHabits([FromQuery] AnalyticsRangeRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await analyticsService.GetHabitsAsync(userId, request, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<HabitAnalyticsResponse>.Success(result.Value!))
            : ToErrorResponse(result);
    }

    [HttpGet("tasks")]
    public async Task<IActionResult> GetTasks([FromQuery] AnalyticsRangeRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await analyticsService.GetTasksAsync(userId, request, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<TaskAnalyticsResponse>.Success(result.Value!))
            : ToErrorResponse(result);
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdValue, out userId);
    }

    private IActionResult ToErrorResponse<T>(Result<T> result)
    {
        var response = ApiResponse<object>.Failure(result.Error ?? "Request failed.");

        return result.ErrorType switch
        {
            ErrorType.Conflict => Conflict(response),
            ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, response),
            ErrorType.Unauthorized => Unauthorized(response),
            ErrorType.NotFound => NotFound(response),
            _ => BadRequest(response)
        };
    }
}
