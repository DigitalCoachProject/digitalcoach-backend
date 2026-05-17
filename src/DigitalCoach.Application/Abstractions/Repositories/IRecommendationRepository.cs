using DigitalCoach.Domain.Entities;

namespace DigitalCoach.Application.Abstractions.Repositories;

public interface IRecommendationRepository : IRepository<Recommendation>
{
    Task<IReadOnlyList<Recommendation>> ListByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsForTodayAsync(int userId, string type, string message, DateTime utcNow, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Recommendation> recommendations, CancellationToken cancellationToken = default);
}
