using DigitalCoach.Application.Abstractions.Repositories;
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
}
