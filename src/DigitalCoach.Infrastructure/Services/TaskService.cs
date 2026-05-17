using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Application.Abstractions.Services;
using DigitalCoach.Application.Common;
using DigitalCoach.Application.DTOs.Tasks;
using DigitalCoach.Domain.Constants;
using DigitalCoach.Domain.Entities;

namespace DigitalCoach.Infrastructure.Services;

public sealed class TaskService(
    ITaskRepository taskRepository,
    ITaskHistoryRepository taskHistoryRepository) : ITaskService
{
    public async Task<Result<TaskResponse>> CreateAsync(int userId, CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var task = new UserTask
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Description = TrimToNull(request.Description),
            CreatedAt = now,
            PlannedDate = request.PlannedDate,
            Deadline = request.Deadline,
            Priority = request.Priority,
            Difficulty = request.Difficulty,
            Status = request.Status,
            CompletedAt = request.Status == TaskStatuses.Completed ? now : null,
            RescheduleCount = 0,
            UpdatedAt = now
        };

        await taskRepository.AddAsync(task, cancellationToken);
        await taskRepository.SaveChangesAsync(cancellationToken);

        return Result<TaskResponse>.Success(ToResponse(task));
    }

    public async Task<Result<PaginatedResponse<TaskResponse>>> ListAsync(int userId, TaskFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var page = await taskRepository.ListByUserAsync(
            userId,
            filter.Status,
            filter.Priority,
            filter.From,
            filter.To,
            filter.Overdue,
            filter.SortBy,
            filter.SortDescending,
            DateOnly.FromDateTime(DateTime.UtcNow),
            filter.Page,
            filter.PageSize,
            cancellationToken);

        var response = PaginatedResponse<TaskResponse>.Create(
            page.Items.Select(ToResponse).ToList(),
            page.Page,
            page.PageSize,
            page.TotalItems);

        return Result<PaginatedResponse<TaskResponse>>.Success(response);
    }

    public async Task<Result<TaskResponse>> GetByIdAsync(int userId, int taskId, CancellationToken cancellationToken = default)
    {
        var taskResult = await GetOwnedTaskAsync(userId, taskId, cancellationToken);
        return taskResult.Succeeded
            ? Result<TaskResponse>.Success(ToResponse(taskResult.Value!))
            : Result<TaskResponse>.Failure(taskResult.Error!, taskResult.ErrorType);
    }

    public async Task<Result<TaskResponse>> UpdateAsync(int userId, int taskId, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var taskResult = await GetOwnedTaskAsync(userId, taskId, cancellationToken);
        if (!taskResult.Succeeded)
        {
            return Result<TaskResponse>.Failure(taskResult.Error!, taskResult.ErrorType);
        }

        var task = taskResult.Value!;
        task.Name = request.Name.Trim();
        task.Description = TrimToNull(request.Description);
        task.PlannedDate = request.PlannedDate;
        task.Deadline = request.Deadline;
        task.Priority = request.Priority;
        task.Difficulty = request.Difficulty;
        task.Status = request.Status;
        task.CompletedAt = request.Status == TaskStatuses.Completed
            ? task.CompletedAt ?? DateTime.UtcNow
            : null;
        task.UpdatedAt = DateTime.UtcNow;

        taskRepository.Update(task);
        await taskRepository.SaveChangesAsync(cancellationToken);

        return Result<TaskResponse>.Success(ToResponse(task));
    }

    public async Task<Result> DeleteAsync(int userId, int taskId, CancellationToken cancellationToken = default)
    {
        var taskResult = await GetOwnedTaskAsync(userId, taskId, cancellationToken);
        if (!taskResult.Succeeded)
        {
            return Result.Failure(taskResult.Error!, taskResult.ErrorType);
        }

        taskRepository.Remove(taskResult.Value!);
        await taskRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<TaskResponse>> CompleteAsync(int userId, int taskId, CancellationToken cancellationToken = default)
    {
        var taskResult = await GetOwnedTaskAsync(userId, taskId, cancellationToken);
        if (!taskResult.Succeeded)
        {
            return Result<TaskResponse>.Failure(taskResult.Error!, taskResult.ErrorType);
        }

        var task = taskResult.Value!;
        if (task.Status == TaskStatuses.Cancelled)
        {
            return Result<TaskResponse>.Failure("Cancelled tasks cannot be completed.", ErrorType.Conflict);
        }

        if (task.Status == TaskStatuses.Completed)
        {
            return Result<TaskResponse>.Failure("Task is already completed.", ErrorType.Conflict);
        }

        task.Status = TaskStatuses.Completed;
        task.CompletedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        taskRepository.Update(task);
        await taskRepository.SaveChangesAsync(cancellationToken);

        return Result<TaskResponse>.Success(ToResponse(task));
    }

    public async Task<Result<TaskResponse>> CancelAsync(int userId, int taskId, CancellationToken cancellationToken = default)
    {
        var taskResult = await GetOwnedTaskAsync(userId, taskId, cancellationToken);
        if (!taskResult.Succeeded)
        {
            return Result<TaskResponse>.Failure(taskResult.Error!, taskResult.ErrorType);
        }

        var task = taskResult.Value!;
        if (task.Status == TaskStatuses.Completed)
        {
            return Result<TaskResponse>.Failure("Completed tasks cannot be cancelled.", ErrorType.Conflict);
        }

        if (task.Status == TaskStatuses.Cancelled)
        {
            return Result<TaskResponse>.Failure("Task is already cancelled.", ErrorType.Conflict);
        }

        task.Status = TaskStatuses.Cancelled;
        task.CompletedAt = null;
        task.UpdatedAt = DateTime.UtcNow;

        taskRepository.Update(task);
        await taskRepository.SaveChangesAsync(cancellationToken);

        return Result<TaskResponse>.Success(ToResponse(task));
    }

    public async Task<Result<TaskResponse>> RescheduleAsync(int userId, int taskId, RescheduleTaskRequest request, CancellationToken cancellationToken = default)
    {
        var taskResult = await GetOwnedTaskAsync(userId, taskId, cancellationToken);
        if (!taskResult.Succeeded)
        {
            return Result<TaskResponse>.Failure(taskResult.Error!, taskResult.ErrorType);
        }

        var task = taskResult.Value!;
        if (task.Status == TaskStatuses.Completed)
        {
            return Result<TaskResponse>.Failure("Completed tasks cannot be rescheduled.", ErrorType.Conflict);
        }

        if (task.Status == TaskStatuses.Cancelled)
        {
            return Result<TaskResponse>.Failure("Cancelled tasks cannot be rescheduled.", ErrorType.Conflict);
        }

        if (task.Deadline.HasValue && task.Deadline.Value < request.NewPlannedDate)
        {
            return Result<TaskResponse>.Failure("New planned date cannot be later than deadline.", ErrorType.Validation);
        }

        var oldDate = task.PlannedDate;
        task.PlannedDate = request.NewPlannedDate;
        task.RescheduleCount++;
        task.UpdatedAt = DateTime.UtcNow;
        if (task.Status == TaskStatuses.Overdue)
        {
            task.Status = TaskStatuses.Planned;
        }

        var history = new TaskHistory
        {
            TaskId = task.Id,
            ChangeDate = DateTime.UtcNow,
            OldDate = oldDate,
            NewDate = request.NewPlannedDate,
            Reason = TrimToNull(request.Reason)
        };

        taskRepository.Update(task);
        await taskHistoryRepository.AddAsync(history, cancellationToken);
        await taskRepository.SaveChangesAsync(cancellationToken);

        return Result<TaskResponse>.Success(ToResponse(task));
    }

    public async Task<Result<IReadOnlyList<TaskHistoryResponse>>> ListHistoryAsync(int userId, int taskId, CancellationToken cancellationToken = default)
    {
        var taskResult = await GetOwnedTaskAsync(userId, taskId, cancellationToken);
        if (!taskResult.Succeeded)
        {
            return Result<IReadOnlyList<TaskHistoryResponse>>.Failure(taskResult.Error!, taskResult.ErrorType);
        }

        var history = await taskHistoryRepository.ListByTaskAsync(taskId, cancellationToken);
        return Result<IReadOnlyList<TaskHistoryResponse>>.Success(history.Select(ToResponse).ToList());
    }

    private async Task<Result<UserTask>> GetOwnedTaskAsync(int userId, int taskId, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task is null)
        {
            return Result<UserTask>.Failure("Task was not found.", ErrorType.NotFound);
        }

        if (task.UserId != userId)
        {
            return Result<UserTask>.Failure("You do not have permission to access this task.", ErrorType.Forbidden);
        }

        return Result<UserTask>.Success(task);
    }

    private static TaskResponse ToResponse(UserTask task)
    {
        return new TaskResponse(
            task.Id,
            task.UserId,
            task.Name,
            task.Description,
            task.CreatedAt,
            task.PlannedDate,
            task.Deadline,
            task.Priority,
            task.Difficulty,
            task.Status,
            task.CompletedAt,
            task.RescheduleCount,
            task.UpdatedAt);
    }

    private static TaskHistoryResponse ToResponse(TaskHistory history)
    {
        return new TaskHistoryResponse(
            history.Id,
            history.TaskId,
            history.ChangeDate,
            history.OldDate,
            history.NewDate,
            history.Reason);
    }

    private static string? TrimToNull(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
