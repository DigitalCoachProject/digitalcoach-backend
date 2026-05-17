using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Application.Abstractions.Services;
using DigitalCoach.Application.Common;
using DigitalCoach.Application.DTOs.Notifications;
using DigitalCoach.Domain.Constants;
using DigitalCoach.Domain.Entities;

namespace DigitalCoach.Infrastructure.Services;

public sealed class NotificationService(
    INotificationRepository notificationRepository,
    IAnalyticsRepository analyticsRepository,
    IRecommendationRepository recommendationRepository) : INotificationService
{
    public async Task<Result<PaginatedResponse<NotificationResponse>>> ListAsync(int userId, NotificationQueryRequest request, CancellationToken cancellationToken = default)
    {
        var page = await notificationRepository.ListByUserAsync(
            userId,
            request.SortBy,
            request.SortDescending,
            request.Page,
            request.PageSize,
            cancellationToken);

        var response = PaginatedResponse<NotificationResponse>.Create(
            page.Items.Select(ToResponse).ToList(),
            page.Page,
            page.PageSize,
            page.TotalItems);

        return Result<PaginatedResponse<NotificationResponse>>.Success(response);
    }

    public async Task<Result<UnreadNotificationCountResponse>> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default)
    {
        var count = await notificationRepository.CountUnreadByUserAsync(userId, cancellationToken);
        return Result<UnreadNotificationCountResponse>.Success(new UnreadNotificationCountResponse(count));
    }

    public async Task<Result<IReadOnlyList<NotificationResponse>>> GenerateAsync(int userId, CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(utcNow);
        var from = today.AddDays(-29);
        var recentFrom = today.AddDays(-6);

        var habits = await analyticsRepository.ListHabitsAsync(userId, today, cancellationToken);
        var todayHabitLogs = await analyticsRepository.ListHabitLogsAsync(userId, today, today, cancellationToken);
        var tasks = await analyticsRepository.ListTasksAsync(userId, from, today, cancellationToken);
        var recentStates = await analyticsRepository.ListDailyStatesAsync(userId, recentFrom, today, cancellationToken);
        var todayStates = recentStates.Where(x => x.Date == today).ToList();
        var recommendations = await recommendationRepository.ListByUserAsync(userId, cancellationToken);

        var candidates = BuildCandidates(habits, todayHabitLogs, tasks, recentStates, todayStates, recommendations, utcNow);
        var newNotifications = new List<Notification>();

        foreach (var candidate in candidates)
        {
            var exists = await notificationRepository.ExistsForTodayAsync(
                userId,
                candidate.Type,
                candidate.Message,
                utcNow,
                cancellationToken);

            if (exists)
            {
                continue;
            }

            newNotifications.Add(new Notification
            {
                UserId = userId,
                Type = candidate.Type,
                Title = candidate.Title,
                Message = candidate.Message,
                Priority = candidate.Priority,
                IsRead = false,
                CreatedAt = utcNow,
                ScheduledFor = candidate.ScheduledFor,
                ReadAt = null,
                ExpiresAt = candidate.ExpiresAt
            });
        }

        if (newNotifications.Count > 0)
        {
            await notificationRepository.AddRangeAsync(newNotifications, cancellationToken);
            await notificationRepository.SaveChangesAsync(cancellationToken);
        }

        return Result<IReadOnlyList<NotificationResponse>>.Success(newNotifications.Select(ToResponse).ToList());
    }

    public async Task<Result<NotificationResponse>> MarkAsReadAsync(int userId, int notificationId, CancellationToken cancellationToken = default)
    {
        var notificationResult = await GetOwnedNotificationAsync(userId, notificationId, cancellationToken);
        if (!notificationResult.Succeeded)
        {
            return Result<NotificationResponse>.Failure(notificationResult.Error!, notificationResult.ErrorType);
        }

        var notification = notificationResult.Value!;
        var utcNow = DateTime.UtcNow;
        notification.IsRead = true;
        notification.ReadAt ??= utcNow;

        notificationRepository.Update(notification);
        await notificationRepository.SaveChangesAsync(cancellationToken);

        return Result<NotificationResponse>.Success(ToResponse(notification));
    }

    public async Task<Result<int>> MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default)
    {
        var notifications = await notificationRepository.ListUnreadByUserAsync(userId, cancellationToken);
        var utcNow = DateTime.UtcNow;

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt ??= utcNow;
            notificationRepository.Update(notification);
        }

        if (notifications.Count > 0)
        {
            await notificationRepository.SaveChangesAsync(cancellationToken);
        }

        return Result<int>.Success(notifications.Count);
    }

    public async Task<Result> DeleteAsync(int userId, int notificationId, CancellationToken cancellationToken = default)
    {
        var notificationResult = await GetOwnedNotificationAsync(userId, notificationId, cancellationToken);
        if (!notificationResult.Succeeded)
        {
            return Result.Failure(notificationResult.Error!, notificationResult.ErrorType);
        }

        notificationRepository.Remove(notificationResult.Value!);
        await notificationRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result<Notification>> GetOwnedNotificationAsync(int userId, int notificationId, CancellationToken cancellationToken)
    {
        var notification = await notificationRepository.GetByIdAsync(notificationId, cancellationToken);
        if (notification is null)
        {
            return Result<Notification>.Failure("Notification was not found.", ErrorType.NotFound);
        }

        if (notification.UserId != userId)
        {
            return Result<Notification>.Failure("You do not have permission to access this notification.", ErrorType.Forbidden);
        }

        return Result<Notification>.Success(notification);
    }

    private static IReadOnlyList<NotificationCandidate> BuildCandidates(
        IReadOnlyList<Habit> habits,
        IReadOnlyList<HabitLog> todayHabitLogs,
        IReadOnlyList<UserTask> tasks,
        IReadOnlyList<DailyState> recentStates,
        IReadOnlyList<DailyState> todayStates,
        IReadOnlyList<Recommendation> recommendations,
        DateTime utcNow)
    {
        var candidates = new List<NotificationCandidate>();
        var activeHabits = habits.Where(x => x.IsActive).ToList();
        var expiresAt = utcNow.AddDays(7);

        AddHabitReminder(candidates, activeHabits, todayHabitLogs, expiresAt);
        AddOverdueTasks(candidates, tasks, expiresAt);
        AddBurnoutRisk(candidates, recentStates, expiresAt);
        AddWellnessInactivity(candidates, recentStates, expiresAt);
        AddUnreadRecommendations(candidates, recommendations, expiresAt);
        AddDailyStateMissing(candidates, todayStates, expiresAt);

        return candidates;
    }

    private static void AddHabitReminder(List<NotificationCandidate> candidates, IReadOnlyList<Habit> activeHabits, IReadOnlyList<HabitLog> todayHabitLogs, DateTime expiresAt)
    {
        if (activeHabits.Count == 0 || todayHabitLogs.Count > 0)
        {
            return;
        }

        candidates.Add(new NotificationCandidate(
            NotificationTypes.Habit,
            "Today's habits are waiting",
            "Don't forget today's habits. A small completion now keeps your consistency moving.",
            3,
            null,
            expiresAt));
    }

    private static void AddOverdueTasks(List<NotificationCandidate> candidates, IReadOnlyList<UserTask> tasks, DateTime expiresAt)
    {
        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var overdueTasks = CountOverdueTasks(tasks, currentDate);
        if (overdueTasks == 0)
        {
            return;
        }

        candidates.Add(new NotificationCandidate(
            NotificationTypes.Task,
            "Overdue tasks",
            overdueTasks == 1
                ? "You currently have 1 overdue task that requires attention."
                : $"You currently have {overdueTasks} overdue tasks that require attention.",
            overdueTasks >= 3 ? 4 : 3,
            null,
            expiresAt));
    }

    private static void AddBurnoutRisk(List<NotificationCandidate> candidates, IReadOnlyList<DailyState> recentStates, DateTime expiresAt)
    {
        if (recentStates.Count == 0)
        {
            return;
        }

        var averageStress = AverageOrZero(recentStates.Select(x => (decimal)x.Stress));
        var averageEnergy = AverageOrZero(recentStates.Select(x => (decimal)x.Energy));
        var averageSleepQuality = AverageOrZero(recentStates.Where(x => x.SleepQuality.HasValue).Select(x => (decimal)x.SleepQuality!.Value));

        if (averageStress >= 4m && averageEnergy <= 2m && averageSleepQuality > 0m && averageSleepQuality <= 2m)
        {
            candidates.Add(new NotificationCandidate(
                NotificationTypes.Burnout,
                "Burnout risk is elevated",
                $"Recent wellness logs show high stress ({averageStress}), low energy ({averageEnergy}), and poor sleep quality ({averageSleepQuality}). Reduce workload and schedule recovery time today.",
                5,
                null,
                expiresAt));
        }
    }

    private static void AddWellnessInactivity(List<NotificationCandidate> candidates, IReadOnlyList<DailyState> recentStates, DateTime expiresAt)
    {
        if (recentStates.Count == 0)
        {
            return;
        }

        var averageActivity = AverageOrZero(recentStates.Where(x => x.ActivityDuration.HasValue).Select(x => x.ActivityDuration!.Value));
        var averageScreenTime = AverageOrZero(recentStates.Where(x => x.ScreenTime.HasValue).Select(x => x.ScreenTime!.Value));

        if (averageActivity < 20m && averageScreenTime >= 5m)
        {
            candidates.Add(new NotificationCandidate(
                NotificationTypes.Wellness,
                "Time for movement",
                $"Your recent activity averages {averageActivity} minutes while screen time averages {averageScreenTime} hours. A short walk or stretching break can help reset your energy.",
                3,
                null,
                expiresAt));
        }
    }

    private static void AddUnreadRecommendations(List<NotificationCandidate> candidates, IReadOnlyList<Recommendation> recommendations, DateTime expiresAt)
    {
        var unreadCount = recommendations.Count(x => !x.IsRead);
        if (unreadCount == 0)
        {
            return;
        }

        candidates.Add(new NotificationCandidate(
            NotificationTypes.Recommendation,
            "Unread AI recommendations",
            unreadCount == 1
                ? "You have 1 unread AI recommendation waiting for review."
                : $"You have {unreadCount} unread AI recommendations waiting for review.",
            2,
            null,
            expiresAt));
    }

    private static void AddDailyStateMissing(List<NotificationCandidate> candidates, IReadOnlyList<DailyState> todayStates, DateTime expiresAt)
    {
        if (todayStates.Count > 0)
        {
            return;
        }

        candidates.Add(new NotificationCandidate(
            NotificationTypes.Reminder,
            "Log today's wellness state",
            "Log today's wellness state so analytics and recommendations can stay accurate.",
            3,
            null,
            expiresAt));
    }

    private static int CountOverdueTasks(IReadOnlyList<UserTask> tasks, DateOnly currentDate)
    {
        return tasks.Count(x =>
            x.Status == TaskStatuses.Overdue
            || (x.Deadline.HasValue
                && x.Deadline.Value < currentDate
                && x.Status != TaskStatuses.Completed
                && x.Status != TaskStatuses.Cancelled));
    }

    private static decimal AverageOrZero(IEnumerable<decimal> values)
    {
        var list = values.ToList();
        return list.Count == 0 ? 0m : Math.Round(list.Average(), 2, MidpointRounding.AwayFromZero);
    }

    private static NotificationResponse ToResponse(Notification notification)
    {
        return new NotificationResponse(
            notification.Id,
            notification.Type,
            notification.Title,
            notification.Message,
            notification.Priority,
            notification.IsRead,
            notification.CreatedAt,
            notification.ScheduledFor,
            notification.ReadAt,
            notification.ExpiresAt);
    }

    private sealed record NotificationCandidate(
        string Type,
        string Title,
        string Message,
        int Priority,
        DateTime? ScheduledFor,
        DateTime? ExpiresAt);
}
