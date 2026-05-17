using DigitalCoach.Application.Abstractions.Services;
using DigitalCoach.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DigitalCoach.Infrastructure.Services;

public sealed class RecommendationNotificationBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<RecommendationNotificationBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await GenerateForAllUsersAsync(stoppingToken);

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private async Task GenerateForAllUsersAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Background recommendation generation started.");

            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DigitalCoachDbContext>();
            var recommendationService = scope.ServiceProvider.GetRequiredService<IRecommendationService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var userIds = await dbContext.UserProfiles
                .AsNoTracking()
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            foreach (var userId in userIds)
            {
                try
                {
                    var recommendations = await recommendationService.GenerateAsync(userId, cancellationToken);
                    if (recommendations.Succeeded)
                    {
                        logger.LogInformation("Generated {Count} recommendations for user {UserId}.", recommendations.Value?.Count ?? 0, userId);
                    }
                    else
                    {
                        logger.LogWarning("Recommendation generation failed for user {UserId}: {Error}", userId, recommendations.Error);
                    }

                    var notifications = await notificationService.GenerateAsync(userId, cancellationToken);
                    if (notifications.Succeeded)
                    {
                        logger.LogInformation("Generated {Count} notifications for user {UserId}.", notifications.Value?.Count ?? 0, userId);
                    }
                    else
                    {
                        logger.LogWarning("Notification generation failed for user {UserId}: {Error}", userId, notifications.Error);
                    }
                }
                catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
                {
                    logger.LogError(exception, "Background generation failed for user {UserId}.", userId);
                }
            }

            logger.LogInformation("Background notification generation completed.");
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(exception, "Background recommendation and notification generation failed.");
        }
    }
}
