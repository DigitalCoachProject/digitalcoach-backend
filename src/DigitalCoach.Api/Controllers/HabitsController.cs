using System.Security.Claims;
using DigitalCoach.Api.Common;
using DigitalCoach.Application.Abstractions.Services;
using DigitalCoach.Application.Common;
using DigitalCoach.Application.DTOs.Habits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalCoach.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/habits")]
public sealed class HabitsController(IHabitService habitService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateHabitRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await habitService.CreateAsync(userId, request, cancellationToken);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ApiResponse<HabitResponse>.Success(result.Value))
            : ToErrorResponse(result);
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] HabitFilterRequest filter, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await habitService.ListAsync(userId, filter, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<PaginatedResponse<HabitResponse>>.Success(result.Value!))
            : ToErrorResponse(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await habitService.GetByIdAsync(userId, id, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<HabitResponse>.Success(result.Value!))
            : ToErrorResponse(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateHabitRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await habitService.UpdateAsync(userId, id, request, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<HabitResponse>.Success(result.Value!))
            : ToErrorResponse(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await habitService.DeleteAsync(userId, id, cancellationToken);
        return result.Succeeded
            ? NoContent()
            : ToErrorResponse(result);
    }

    [HttpPost("{id:int}/log")]
    public async Task<IActionResult> CreateLog(int id, CreateHabitLogRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await habitService.CreateLogAsync(userId, id, request, cancellationToken);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetLogs), new { id }, ApiResponse<HabitLogResponse>.Success(result.Value!))
            : ToErrorResponse(result);
    }

    [HttpGet("{id:int}/logs")]
    public async Task<IActionResult> GetLogs(int id, [FromQuery] HabitLogFilterRequest filter, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await habitService.ListLogsAsync(userId, id, filter, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<IReadOnlyList<HabitLogResponse>>.Success(result.Value!))
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
