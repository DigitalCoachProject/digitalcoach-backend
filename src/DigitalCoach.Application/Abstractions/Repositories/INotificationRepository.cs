using DigitalCoach.Application.Common;
using DigitalCoach.Domain.Entities;

namespace DigitalCoach.Application.Abstractions.Repositories;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IReadOnlyList<Notification>> ListByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<Notification>> ListByUserAsync(
        int userId,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> ListUnreadByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<int> CountUnreadByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsForTodayAsync(int userId, string type, string message, DateTime utcNow, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default);
}
