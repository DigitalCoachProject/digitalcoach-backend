using DigitalCoach.Application.DTOs.Analytics;
using DigitalCoach.Application.Common;

namespace DigitalCoach.Application.Abstractions.Services;

public interface IAnalyticsService
{
    Task<Result<DashboardAnalyticsResponse>> GetDashboardAsync(int userId, AnalyticsRangeRequest request, CancellationToken cancellationToken = default);
    Task<Result<ProductivityAnalyticsResponse>> GetProductivityAsync(int userId, AnalyticsRangeRequest request, CancellationToken cancellationToken = default);
    Task<Result<WellnessAnalyticsResponse>> GetWellnessAsync(int userId, AnalyticsRangeRequest request, CancellationToken cancellationToken = default);
    Task<Result<HabitAnalyticsResponse>> GetHabitsAsync(int userId, AnalyticsRangeRequest request, CancellationToken cancellationToken = default);
    Task<Result<TaskAnalyticsResponse>> GetTasksAsync(int userId, AnalyticsRangeRequest request, CancellationToken cancellationToken = default);
}
