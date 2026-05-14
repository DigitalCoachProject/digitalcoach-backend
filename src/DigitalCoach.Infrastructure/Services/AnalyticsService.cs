using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Application.Abstractions.Services;
using DigitalCoach.Application.DTOs.Analytics;

namespace DigitalCoach.Infrastructure.Services;

public sealed class AnalyticsService(IProductivityOverviewRepository productivityOverviewRepository) : IAnalyticsService
{
    public async Task<IReadOnlyList<ProductivityOverviewDto>> GetProductivityOverviewAsync(int userId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        var rows = await productivityOverviewRepository.ListByUserAsync(userId, from, to, cancellationToken);

        return rows
            .Select(x => new ProductivityOverviewDto(
                x.UserId,
                x.Date,
                x.Energy,
                x.Mood,
                x.Stress,
                x.PhysicalState,
                x.TasksCount,
                x.CompletedTasks,
                x.HabitLogs,
                x.CompletedHabits))
            .ToList();
    }
}
