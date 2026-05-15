using DigitalCoach.Domain.Entities;

namespace DigitalCoach.Application.Abstractions.Repositories;

public interface IHabitRepository : IRepository<Habit>
{
    Task<IReadOnlyList<Habit>> ListByUserAsync(
        int userId,
        string? type,
        bool? isActive,
        DateOnly? startDateFrom,
        DateOnly? startDateTo,
        CancellationToken cancellationToken = default);
}
