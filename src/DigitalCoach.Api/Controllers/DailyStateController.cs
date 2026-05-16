using System.Security.Claims;
using DigitalCoach.Api.Common;
using DigitalCoach.Application.Abstractions.Services;
using DigitalCoach.Application.Common;
using DigitalCoach.Application.DTOs.Analytics;
using DigitalCoach.Application.DTOs.DailyStates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalCoach.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/daily-states")]
public sealed class DailyStateController(IDailyStateService dailyStateService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateDailyStateRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await dailyStateService.CreateAsync(userId, request, cancellationToken);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ApiResponse<DailyStateResponse>.Success(result.Value))
            : ToErrorResponse(result);
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DailyStateFilterRequest filter, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await dailyStateService.ListAsync(userId, filter, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<IReadOnlyList<DailyStateResponse>>.Success(result.Value!))
            : ToErrorResponse(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await dailyStateService.GetByIdAsync(userId, id, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<DailyStateResponse>.Success(result.Value!))
            : ToErrorResponse(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateDailyStateRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await dailyStateService.UpdateAsync(userId, id, request, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<DailyStateResponse>.Success(result.Value!))
            : ToErrorResponse(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await dailyStateService.DeleteAsync(userId, id, cancellationToken);
        return result.Succeeded
            ? NoContent()
            : ToErrorResponse(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] DailyStateRangeRequest range, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await dailyStateService.GetSummaryAsync(userId, range, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<DailyStateSummaryResponse>.Success(result.Value!))
            : ToErrorResponse(result);
    }

    [HttpGet("productivity-overview")]
    public async Task<IActionResult> GetProductivityOverview([FromQuery] DailyStateRangeRequest range, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await dailyStateService.GetProductivityOverviewAsync(userId, range, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<IReadOnlyList<ProductivityOverviewDto>>.Success(result.Value!))
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
