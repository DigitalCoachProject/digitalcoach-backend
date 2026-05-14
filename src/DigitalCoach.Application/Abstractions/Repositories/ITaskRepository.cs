using DigitalCoach.Domain.Entities;

namespace DigitalCoach.Application.Abstractions.Repositories;

public interface ITaskRepository : IRepository<UserTask>
{
    Task<IReadOnlyList<UserTask>> ListByUserAsync(int userId, CancellationToken cancellationToken = default);
}
