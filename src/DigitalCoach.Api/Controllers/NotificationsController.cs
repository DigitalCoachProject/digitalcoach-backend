using System.Security.Claims;
using DigitalCoach.Api.Common;
using DigitalCoach.Application.Abstractions.Services;
using DigitalCoach.Application.Common;
using DigitalCoach.Application.DTOs.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalCoach.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public sealed class NotificationsController(INotificationService notificationService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await notificationService.ListAsync(userId, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<IReadOnlyList<NotificationResponse>>.Success(result.Value!))
            : ToErrorResponse(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await notificationService.GetUnreadCountAsync(userId, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<UnreadNotificationCountResponse>.Success(result.Value!))
            : ToErrorResponse(result);
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await notificationService.GenerateAsync(userId, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<IReadOnlyList<NotificationResponse>>.Success(result.Value!))
            : ToErrorResponse(result);
    }

    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await notificationService.MarkAsReadAsync(userId, id, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<NotificationResponse>.Success(result.Value!))
            : ToErrorResponse(result);
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await notificationService.MarkAllAsReadAsync(userId, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<object>.Success(new { count = result.Value }))
            : ToErrorResponse(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await notificationService.DeleteAsync(userId, id, cancellationToken);
        return result.Succeeded
            ? NoContent()
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

    private IActionResult ToErrorResponse(Result result)
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
