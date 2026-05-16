using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Domain.Entities;
using DigitalCoach.Domain.Views;
using DigitalCoach.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DigitalCoach.Infrastructure.Repositories;

public sealed class AnalyticsRepository(DigitalCoachDbContext dbContext) : IAnalyticsRepository
{
    public async Task<IReadOnlyList<Habit>> ListHabitsAsync(int userId, DateOnly to, CancellationToken cancellationToken = default)
    {
        return await dbContext.Habits
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.StartDate <= to)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<HabitLog>> ListHabitLogsAsync(int userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        return await dbContext.HabitLogs
            .AsNoTracking()
            .Include(x => x.Habit)
            .Where(x => x.Habit.UserId == userId && x.Date >= from && x.Date <= to)
            .OrderBy(x => x.Date)
            .ThenBy(x => x.HabitId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserTask>> ListTasksAsync(int userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        return await dbContext.Tasks
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.PlannedDate >= from && x.PlannedDate <= to)
            .OrderBy(x => x.PlannedDate)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TaskHistory>> ListTaskHistoryAsync(int userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.ToDateTime(TimeOnly.MaxValue);

        return await dbContext.TaskHistories
            .AsNoTracking()
            .Include(x => x.Task)
            .Where(x => x.Task.UserId == userId && x.ChangeDate >= fromDateTime && x.ChangeDate <= toDateTime)
            .OrderByDescending(x => x.ChangeDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DailyState>> ListDailyStatesAsync(int userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        return await dbContext.DailyStates
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Date >= from && x.Date <= to)
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductivityOverview>> ListProductivityOverviewAsync(int userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        return await dbContext.ProductivityOverview
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Date >= from && x.Date <= to)
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);
    }
}
