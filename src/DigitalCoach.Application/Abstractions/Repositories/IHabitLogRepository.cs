using DigitalCoach.Domain.Entities;

namespace DigitalCoach.Application.Abstractions.Repositories;

public interface IHabitLogRepository : IRepository<HabitLog>
{
    Task<IReadOnlyList<HabitLog>> ListByHabitAsync(
        int habitId,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken = default);

    Task<HabitLog?> GetByHabitAndDateAsync(int habitId, DateOnly date, CancellationToken cancellationToken = default);
}
