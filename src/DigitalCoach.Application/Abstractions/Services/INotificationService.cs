using DigitalCoach.Application.Common;
using DigitalCoach.Application.DTOs.Notifications;

namespace DigitalCoach.Application.Abstractions.Services;

public interface INotificationService
{
    Task<Result<PaginatedResponse<NotificationResponse>>> ListAsync(int userId, NotificationQueryRequest request, CancellationToken cancellationToken = default);
    Task<Result<UnreadNotificationCountResponse>> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<NotificationResponse>>> GenerateAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result<NotificationResponse>> MarkAsReadAsync(int userId, int notificationId, CancellationToken cancellationToken = default);
    Task<Result<int>> MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int userId, int notificationId, CancellationToken cancellationToken = default);
}
