using DigitalCoach.Application.Common;
using DigitalCoach.Application.DTOs.Analytics;
using DigitalCoach.Application.DTOs.DailyStates;

namespace DigitalCoach.Application.Abstractions.Services;

public interface IDailyStateService
{
    Task<Result<DailyStateResponse>> CreateAsync(int userId, CreateDailyStateRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<DailyStateResponse>>> ListAsync(int userId, DailyStateFilterRequest filter, CancellationToken cancellationToken = default);
    Task<Result<DailyStateResponse>> GetByIdAsync(int userId, int dailyStateId, CancellationToken cancellationToken = default);
    Task<Result<DailyStateResponse>> UpdateAsync(int userId, int dailyStateId, UpdateDailyStateRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int userId, int dailyStateId, CancellationToken cancellationToken = default);
    Task<Result<DailyStateSummaryResponse>> GetSummaryAsync(int userId, DailyStateRangeRequest range, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ProductivityOverviewDto>>> GetProductivityOverviewAsync(int userId, DailyStateRangeRequest range, CancellationToken cancellationToken = default);
}
