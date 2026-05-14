using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Domain.Entities;
using DigitalCoach.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DigitalCoach.Infrastructure.Repositories;

public sealed class HabitRepository(DigitalCoachDbContext dbContext)
    : Repository<Habit>(dbContext), IHabitRepository
{
    public async Task<IReadOnlyList<Habit>> ListByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Habits.AsNoTracking().Where(x => x.UserId == userId).ToListAsync(cancellationToken);
    }
}
