using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Domain.Entities;
using DigitalCoach.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DigitalCoach.Infrastructure.Repositories;

public sealed class HabitLogRepository(DigitalCoachDbContext dbContext)
    : Repository<HabitLog>(dbContext), IHabitLogRepository
{
    public async Task<IReadOnlyList<HabitLog>> ListByHabitAsync(
        int habitId,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken = default)
    {
        var query = DbContext.HabitLogs
            .AsNoTracking()
            .Where(x => x.HabitId == habitId);

        if (from.HasValue)
        {
            query = query.Where(x => x.Date >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.Date <= to.Value);
        }

        return await query
            .OrderByDescending(x => x.Date)
            .ToListAsync(cancellationToken);
    }

    public Task<HabitLog?> GetByHabitAndDateAsync(int habitId, DateOnly date, CancellationToken cancellationToken = default)
    {
        return DbContext.HabitLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.HabitId == habitId && x.Date == date, cancellationToken);
    }
}
