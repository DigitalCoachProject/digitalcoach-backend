using DigitalCoach.Domain.Entities;

namespace DigitalCoach.Application.Abstractions.Repositories;

public interface IDailyStateRepository : IRepository<DailyState>
{
    Task<DailyState?> GetByUserAndDateAsync(int userId, DateOnly date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DailyState>> ListByUserAsync(
        int userId,
        DateOnly? from,
        DateOnly? to,
        int? mood,
        int? stress,
        int? energy,
        int? physicalState,
        bool? hasIllness,
        bool? hasPainOrInjury,
        string? activityType,
        CancellationToken cancellationToken = default);
}
