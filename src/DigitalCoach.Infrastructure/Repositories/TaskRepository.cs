using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Domain.Constants;
using DigitalCoach.Domain.Entities;
using DigitalCoach.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DigitalCoach.Infrastructure.Repositories;

public sealed class TaskRepository(DigitalCoachDbContext dbContext)
    : Repository<UserTask>(dbContext), ITaskRepository
{
    public async Task<IReadOnlyList<UserTask>> ListByUserAsync(
        int userId,
        string? status,
        int? priority,
        DateOnly? from,
        DateOnly? to,
        bool? overdue,
        string? sortBy,
        bool sortDescending,
        DateOnly currentDate,
        CancellationToken cancellationToken = default)
    {
        var query = DbContext.Tasks
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (priority.HasValue)
        {
            query = query.Where(x => x.Priority == priority.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(x => x.PlannedDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.PlannedDate <= to.Value);
        }

        if (overdue.HasValue)
        {
            query = overdue.Value
                ? query.Where(x =>
                    x.Status == TaskStatuses.Overdue
                    || (x.Deadline.HasValue
                        && x.Deadline.Value < currentDate
                        && x.Status != TaskStatuses.Completed
                        && x.Status != TaskStatuses.Cancelled))
                : query.Where(x =>
                    x.Status != TaskStatuses.Overdue
                    && (!x.Deadline.HasValue
                        || x.Deadline.Value >= currentDate
                        || x.Status == TaskStatuses.Completed
                        || x.Status == TaskStatuses.Cancelled));
        }

        query = sortBy switch
        {
            "deadline" => sortDescending
                ? query.OrderByDescending(x => x.Deadline).ThenByDescending(x => x.PlannedDate)
                : query.OrderBy(x => x.Deadline).ThenBy(x => x.PlannedDate),
            _ => sortDescending
                ? query.OrderByDescending(x => x.PlannedDate).ThenByDescending(x => x.Priority)
                : query.OrderBy(x => x.PlannedDate).ThenByDescending(x => x.Priority)
        };

        return await query.ToListAsync(cancellationToken);
    }
}
