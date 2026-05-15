using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Domain.Entities;
using DigitalCoach.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DigitalCoach.Infrastructure.Repositories;

public sealed class HabitRepository(DigitalCoachDbContext dbContext)
    : Repository<Habit>(dbContext), IHabitRepository
{
    public async Task<IReadOnlyList<Habit>> ListByUserAsync(
        int userId,
        string? type,
        bool? isActive,
        DateOnly? startDateFrom,
        DateOnly? startDateTo,
        CancellationToken cancellationToken = default)
    {
        var query = DbContext.Habits
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(x => x.Type == type);
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        if (startDateFrom.HasValue)
        {
            query = query.Where(x => x.StartDate >= startDateFrom.Value);
        }

        if (startDateTo.HasValue)
        {
            query = query.Where(x => x.StartDate <= startDateTo.Value);
        }

        return await query
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.StartDate)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
