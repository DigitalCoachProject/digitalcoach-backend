using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Domain.Entities;
using DigitalCoach.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DigitalCoach.Infrastructure.Repositories;

public sealed class DailyStateRepository(DigitalCoachDbContext dbContext)
    : Repository<DailyState>(dbContext), IDailyStateRepository
{
    public Task<DailyState?> GetByUserAndDateAsync(int userId, DateOnly date, CancellationToken cancellationToken = default)
    {
        return DbContext.DailyStates.FirstOrDefaultAsync(x => x.UserId == userId && x.Date == date, cancellationToken);
    }

    public async Task<IReadOnlyList<DailyState>> ListByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await DbContext.DailyStates.AsNoTracking().Where(x => x.UserId == userId).ToListAsync(cancellationToken);
    }
}
