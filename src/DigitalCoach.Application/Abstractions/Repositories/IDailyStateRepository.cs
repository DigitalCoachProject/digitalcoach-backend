using DigitalCoach.Domain.Entities;

namespace DigitalCoach.Application.Abstractions.Repositories;

public interface IDailyStateRepository : IRepository<DailyState>
{
    Task<DailyState?> GetByUserAndDateAsync(int userId, DateOnly date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DailyState>> ListByUserAsync(int userId, CancellationToken cancellationToken = default);
}
