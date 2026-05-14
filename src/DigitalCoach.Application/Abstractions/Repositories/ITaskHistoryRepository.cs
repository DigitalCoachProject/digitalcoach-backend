using DigitalCoach.Domain.Entities;

namespace DigitalCoach.Application.Abstractions.Repositories;

public interface ITaskHistoryRepository : IRepository<TaskHistory>
{
    Task<IReadOnlyList<TaskHistory>> ListByTaskAsync(int taskId, CancellationToken cancellationToken = default);
}
