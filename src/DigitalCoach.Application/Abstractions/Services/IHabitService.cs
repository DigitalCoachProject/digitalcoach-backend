using DigitalCoach.Application.Common;
using DigitalCoach.Application.DTOs.Habits;

namespace DigitalCoach.Application.Abstractions.Services;

public interface IHabitService
{
    Task<Result<HabitResponse>> CreateAsync(int userId, CreateHabitRequest request, CancellationToken cancellationToken = default);
    Task<Result<PaginatedResponse<HabitResponse>>> ListAsync(int userId, HabitFilterRequest filter, CancellationToken cancellationToken = default);
    Task<Result<HabitResponse>> GetByIdAsync(int userId, int habitId, CancellationToken cancellationToken = default);
    Task<Result<HabitResponse>> UpdateAsync(int userId, int habitId, UpdateHabitRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int userId, int habitId, CancellationToken cancellationToken = default);
    Task<Result<HabitLogResponse>> CreateLogAsync(int userId, int habitId, CreateHabitLogRequest request, CancellationToken cancellationToken = default);
    Task<Result<HabitLogResponse>> UpsertLogAsync(int userId, int habitId, CreateHabitLogRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteLogAsync(int userId, int habitId, DateOnly date, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<HabitLogResponse>>> ListLogsAsync(int userId, int habitId, HabitLogFilterRequest filter, CancellationToken cancellationToken = default);
}
