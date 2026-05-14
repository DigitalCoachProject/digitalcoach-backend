using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Domain.Entities;
using DigitalCoach.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DigitalCoach.Infrastructure.Repositories;

public sealed class TaskHistoryRepository(DigitalCoachDbContext dbContext)
    : Repository<TaskHistory>(dbContext), ITaskHistoryRepository
{
    public async Task<IReadOnlyList<TaskHistory>> ListByTaskAsync(int taskId, CancellationToken cancellationToken = default)
    {
        return await DbContext.TaskHistories.AsNoTracking().Where(x => x.TaskId == taskId).ToListAsync(cancellationToken);
    }
}
