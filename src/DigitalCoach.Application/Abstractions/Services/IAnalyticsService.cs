using DigitalCoach.Application.DTOs.Analytics;

namespace DigitalCoach.Application.Abstractions.Services;

public interface IAnalyticsService
{
    Task<IReadOnlyList<ProductivityOverviewDto>> GetProductivityOverviewAsync(int userId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);
}
