using System.Security.Claims;
using DigitalCoach.Api.Common;
using DigitalCoach.Application.Abstractions.Services;
using DigitalCoach.Application.Common;
using DigitalCoach.Application.DTOs.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalCoach.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
public sealed class TasksController(ITaskService taskService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await taskService.CreateAsync(userId, request, cancellationToken);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ApiResponse<TaskResponse>.Success(result.Value))
            : ToErrorResponse(result);
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] TaskFilterRequest filter, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await taskService.ListAsync(userId, filter, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<PaginatedResponse<TaskResponse>>.Success(result.Value!))
            : ToErrorResponse(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await taskService.GetByIdAsync(userId, id, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<TaskResponse>.Success(result.Value!))
            : ToErrorResponse(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await taskService.UpdateAsync(userId, id, request, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<TaskResponse>.Success(result.Value!))
            : ToErrorResponse(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await taskService.DeleteAsync(userId, id, cancellationToken);
        return result.Succeeded
            ? NoContent()
            : ToErrorResponse(result);
    }

    [HttpPost("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await taskService.CompleteAsync(userId, id, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<TaskResponse>.Success(result.Value!))
            : ToErrorResponse(result);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await taskService.CancelAsync(userId, id, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<TaskResponse>.Success(result.Value!))
            : ToErrorResponse(result);
    }

    [HttpPost("{id:int}/reschedule")]
    public async Task<IActionResult> Reschedule(int id, RescheduleTaskRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await taskService.RescheduleAsync(userId, id, request, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<TaskResponse>.Success(result.Value!))
            : ToErrorResponse(result);
    }

    [HttpGet("{id:int}/history")]
    public async Task<IActionResult> GetHistory(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token."));
        }

        var result = await taskService.ListHistoryAsync(userId, id, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<IReadOnlyList<TaskHistoryResponse>>.Success(result.Value!))
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
