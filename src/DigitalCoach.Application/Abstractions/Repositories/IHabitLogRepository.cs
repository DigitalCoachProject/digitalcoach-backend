using DigitalCoach.Domain.Entities;

namespace DigitalCoach.Application.Abstractions.Repositories;

public interface IHabitLogRepository : IRepository<HabitLog>
{
    Task<IReadOnlyList<HabitLog>> ListByHabitAsync(int habitId, CancellationToken cancellationToken = default);
}
