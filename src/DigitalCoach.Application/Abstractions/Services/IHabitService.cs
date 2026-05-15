using DigitalCoach.Application.Common;
using DigitalCoach.Application.DTOs.Habits;

namespace DigitalCoach.Application.Abstractions.Services;

public interface IHabitService
{
    Task<Result<HabitResponse>> CreateAsync(int userId, CreateHabitRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<HabitResponse>>> ListAsync(int userId, HabitFilterRequest filter, CancellationToken cancellationToken = default);
    Task<Result<HabitResponse>> GetByIdAsync(int userId, int habitId, CancellationToken cancellationToken = default);
    Task<Result<HabitResponse>> UpdateAsync(int userId, int habitId, UpdateHabitRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int userId, int habitId, CancellationToken cancellationToken = default);
    Task<Result<HabitLogResponse>> CreateLogAsync(int userId, int habitId, CreateHabitLogRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<HabitLogResponse>>> ListLogsAsync(int userId, int habitId, HabitLogFilterRequest filter, CancellationToken cancellationToken = default);
}
