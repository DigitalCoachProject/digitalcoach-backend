using DigitalCoach.Domain.Entities;
using DigitalCoach.Domain.Views;

namespace DigitalCoach.Application.Abstractions.Repositories;

public interface IAnalyticsRepository
{
    Task<IReadOnlyList<Habit>> ListHabitsAsync(int userId, DateOnly to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HabitLog>> ListHabitLogsAsync(int userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserTask>> ListTasksAsync(int userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskHistory>> ListTaskHistoryAsync(int userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DailyState>> ListDailyStatesAsync(int userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductivityOverview>> ListProductivityOverviewAsync(int userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
}
