using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Domain.Entities;
using DigitalCoach.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DigitalCoach.Infrastructure.Repositories;

public sealed class RecommendationRepository(DigitalCoachDbContext dbContext)
    : Repository<Recommendation>(dbContext), IRecommendationRepository
{
    public async Task<IReadOnlyList<Recommendation>> ListByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        return await DbContext.Recommendations
            .AsNoTracking()
            .Where(x => x.UserId == userId && (!x.ExpiresAt.HasValue || x.ExpiresAt.Value > utcNow))
            .OrderBy(x => x.IsRead)
            .ThenByDescending(x => x.Priority)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsForTodayAsync(int userId, string type, string message, DateTime utcNow, CancellationToken cancellationToken = default)
    {
        var startOfDay = utcNow.Date;
        var endOfDay = startOfDay.AddDays(1);

        return DbContext.Recommendations
            .AsNoTracking()
            .AnyAsync(
                x => x.UserId == userId
                    && x.Type == type
                    && x.Message == message
                    && x.CreatedAt >= startOfDay
                    && x.CreatedAt < endOfDay,
                cancellationToken);
    }

    public Task AddRangeAsync(IEnumerable<Recommendation> recommendations, CancellationToken cancellationToken = default)
    {
        return DbContext.Recommendations.AddRangeAsync(recommendations, cancellationToken);
    }
}
