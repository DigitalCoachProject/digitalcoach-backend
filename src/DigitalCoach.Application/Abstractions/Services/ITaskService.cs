using DigitalCoach.Application.Common;
using DigitalCoach.Application.DTOs.Tasks;

namespace DigitalCoach.Application.Abstractions.Services;

public interface ITaskService
{
    Task<Result<TaskResponse>> CreateAsync(int userId, CreateTaskRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<TaskResponse>>> ListAsync(int userId, TaskFilterRequest filter, CancellationToken cancellationToken = default);
    Task<Result<TaskResponse>> GetByIdAsync(int userId, int taskId, CancellationToken cancellationToken = default);
    Task<Result<TaskResponse>> UpdateAsync(int userId, int taskId, UpdateTaskRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int userId, int taskId, CancellationToken cancellationToken = default);
    Task<Result<TaskResponse>> CompleteAsync(int userId, int taskId, CancellationToken cancellationToken = default);
    Task<Result<TaskResponse>> CancelAsync(int userId, int taskId, CancellationToken cancellationToken = default);
    Task<Result<TaskResponse>> RescheduleAsync(int userId, int taskId, RescheduleTaskRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<TaskHistoryResponse>>> ListHistoryAsync(int userId, int taskId, CancellationToken cancellationToken = default);
}
