using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Domain.Entities;
using DigitalCoach.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DigitalCoach.Infrastructure.Repositories;

public sealed class HabitLogRepository(DigitalCoachDbContext dbContext)
    : Repository<HabitLog>(dbContext), IHabitLogRepository
{
    public async Task<IReadOnlyList<HabitLog>> ListByHabitAsync(int habitId, CancellationToken cancellationToken = default)
    {
        return await DbContext.HabitLogs.AsNoTracking().Where(x => x.HabitId == habitId).ToListAsync(cancellationToken);
    }
}
