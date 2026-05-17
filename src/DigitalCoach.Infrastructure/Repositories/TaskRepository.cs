using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Application.Common;
using DigitalCoach.Domain.Constants;
using DigitalCoach.Domain.Entities;
using DigitalCoach.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DigitalCoach.Infrastructure.Repositories;

public sealed class TaskRepository(DigitalCoachDbContext dbContext)
    : Repository<UserTask>(dbContext), ITaskRepository
{
    public async Task<PaginatedResponse<UserTask>> ListByUserAsync(
        int userId,
        string? status,
        int? priority,
        DateOnly? from,
        DateOnly? to,
        bool? overdue,
        string? sortBy,
        bool sortDescending,
        DateOnly currentDate,
        int page,
        int pageSize,
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

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await ApplySorting(query, sortBy, sortDescending)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PaginatedResponse<UserTask>.Create(items, page, pageSize, totalItems);
    }

    private static IQueryable<UserTask> ApplySorting(IQueryable<UserTask> query, string? sortBy, bool sortDescending)
    {
        return sortBy switch
        {
            "created_at" => sortDescending
                ? query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
                : query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id),
            "deadline" => sortDescending
                ? query.OrderByDescending(x => x.Deadline).ThenByDescending(x => x.PlannedDate)
                : query.OrderBy(x => x.Deadline).ThenBy(x => x.PlannedDate),
            "priority" => sortDescending
                ? query.OrderByDescending(x => x.Priority).ThenBy(x => x.PlannedDate)
                : query.OrderBy(x => x.Priority).ThenBy(x => x.PlannedDate),
            "difficulty" => sortDescending
                ? query.OrderByDescending(x => x.Difficulty).ThenBy(x => x.PlannedDate)
                : query.OrderBy(x => x.Difficulty).ThenBy(x => x.PlannedDate),
            _ => sortDescending
                ? query.OrderByDescending(x => x.PlannedDate).ThenByDescending(x => x.Priority)
                : query.OrderBy(x => x.PlannedDate).ThenByDescending(x => x.Priority)
        };
    }
}
