using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Domain.Entities;
using DigitalCoach.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DigitalCoach.Infrastructure.Repositories;

public sealed class DailyStateRepository(DigitalCoachDbContext dbContext)
    : Repository<DailyState>(dbContext), IDailyStateRepository
{
    public Task<DailyState?> GetByUserAndDateAsync(int userId, DateOnly date, CancellationToken cancellationToken = default)
    {
        return DbContext.DailyStates.FirstOrDefaultAsync(x => x.UserId == userId && x.Date == date, cancellationToken);
    }

    public async Task<IReadOnlyList<DailyState>> ListByUserAsync(
        int userId,
        DateOnly? from,
        DateOnly? to,
        int? mood,
        int? stress,
        int? energy,
        int? physicalState,
        bool? hasIllness,
        bool? hasPainOrInjury,
        string? activityType,
        CancellationToken cancellationToken = default)
    {
        var query = DbContext.DailyStates
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (from.HasValue)
        {
            query = query.Where(x => x.Date >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.Date <= to.Value);
        }

        if (mood.HasValue)
        {
            query = query.Where(x => x.Mood == mood.Value);
        }

        if (stress.HasValue)
        {
            query = query.Where(x => x.Stress == stress.Value);
        }

        if (energy.HasValue)
        {
            query = query.Where(x => x.Energy == energy.Value);
        }

        if (physicalState.HasValue)
        {
            query = query.Where(x => x.PhysicalState == physicalState.Value);
        }

        if (hasIllness.HasValue)
        {
            query = query.Where(x => x.HasIllness == hasIllness.Value);
        }

        if (hasPainOrInjury.HasValue)
        {
            query = query.Where(x => x.HasPainOrInjury == hasPainOrInjury.Value);
        }

        if (!string.IsNullOrWhiteSpace(activityType))
        {
            query = query.Where(x => x.ActivityType == activityType);
        }

        return await query
            .OrderByDescending(x => x.Date)
            .ToListAsync(cancellationToken);
    }
}
