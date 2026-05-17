using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Application.Common;
using DigitalCoach.Domain.Entities;
using DigitalCoach.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DigitalCoach.Infrastructure.Repositories;

public sealed class NotificationRepository(DigitalCoachDbContext dbContext)
    : Repository<Notification>(dbContext), INotificationRepository
{
    public async Task<IReadOnlyList<Notification>> ListByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        return await DbContext.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId && (!x.ExpiresAt.HasValue || x.ExpiresAt.Value > utcNow))
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaginatedResponse<Notification>> ListByUserAsync(
        int userId,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        var query = DbContext.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId && (!x.ExpiresAt.HasValue || x.ExpiresAt.Value > utcNow));

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await ApplySorting(query, sortBy, sortDescending)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PaginatedResponse<Notification>.Create(items, page, pageSize, totalItems);
    }

    public async Task<IReadOnlyList<Notification>> ListUnreadByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        return await DbContext.Notifications
            .Where(x => x.UserId == userId && !x.IsRead && (!x.ExpiresAt.HasValue || x.ExpiresAt.Value > utcNow))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountUnreadByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        return DbContext.Notifications
            .AsNoTracking()
            .CountAsync(x => x.UserId == userId && !x.IsRead && (!x.ExpiresAt.HasValue || x.ExpiresAt.Value > utcNow), cancellationToken);
    }

    public Task<bool> ExistsForTodayAsync(int userId, string type, string message, DateTime utcNow, CancellationToken cancellationToken = default)
    {
        var startOfDay = utcNow.Date;
        var endOfDay = startOfDay.AddDays(1);

        return DbContext.Notifications
            .AsNoTracking()
            .AnyAsync(
                x => x.UserId == userId
                    && x.Type == type
                    && x.Message == message
                    && x.CreatedAt >= startOfDay
                    && x.CreatedAt < endOfDay,
                cancellationToken);
    }

    public Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default)
    {
        return DbContext.Notifications.AddRangeAsync(notifications, cancellationToken);
    }

    private static IQueryable<Notification> ApplySorting(IQueryable<Notification> query, string? sortBy, bool sortDescending)
    {
        return sortBy switch
        {
            "created_at" => sortDescending
                ? query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
                : query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id),
            "priority" => sortDescending
                ? query.OrderByDescending(x => x.Priority).ThenByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.Priority).ThenByDescending(x => x.CreatedAt),
            "is_read" => sortDescending
                ? query.OrderByDescending(x => x.IsRead).ThenByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.IsRead).ThenByDescending(x => x.CreatedAt),
            _ => query
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Priority)
        };
    }
}
