using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Domain.Entities;
using DigitalCoach.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DigitalCoach.Infrastructure.Repositories;

public sealed class TaskRepository(DigitalCoachDbContext dbContext)
    : Repository<UserTask>(dbContext), ITaskRepository
{
    public async Task<IReadOnlyList<UserTask>> ListByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Tasks.AsNoTracking().Where(x => x.UserId == userId).ToListAsync(cancellationToken);
    }
}
