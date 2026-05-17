using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Application.Abstractions.Services;
using DigitalCoach.Application.Common;
using DigitalCoach.Application.DTOs.Recommendations;
using DigitalCoach.Domain.Constants;
using DigitalCoach.Domain.Entities;

namespace DigitalCoach.Infrastructure.Services;

public sealed class RecommendationService(
    IRecommendationRepository recommendationRepository,
    IAnalyticsRepository analyticsRepository) : IRecommendationService
{
    public async Task<Result<PaginatedResponse<RecommendationResponse>>> ListAsync(int userId, RecommendationQueryRequest request, CancellationToken cancellationToken = default)
    {
        var page = await recommendationRepository.ListByUserAsync(
            userId,
            request.SortBy,
            request.SortDescending,
            request.Page,
            request.PageSize,
            cancellationToken);

        var response = PaginatedResponse<RecommendationResponse>.Create(
            page.Items.Select(ToResponse).ToList(),
            page.Page,
            page.PageSize,
            page.TotalItems);

        return Result<PaginatedResponse<RecommendationResponse>>.Success(response);
    }

    public async Task<Result<IReadOnlyList<RecommendationResponse>>> GenerateAsync(int userId, CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        var to = DateOnly.FromDateTime(utcNow);
        var from = to.AddDays(-29);
        var recentFrom = to.AddDays(-6);

        var habits = await analyticsRepository.ListHabitsAsync(userId, to, cancellationToken);
        var habitLogs = await analyticsRepository.ListHabitLogsAsync(userId, from, to, cancellationToken);
        var tasks = await analyticsRepository.ListTasksAsync(userId, from, to, cancellationToken);
        var taskHistory = await analyticsRepository.ListTaskHistoryAsync(userId, from, to, cancellationToken);
        var dailyStates = await analyticsRepository.ListDailyStatesAsync(userId, from, to, cancellationToken);

        var candidates = BuildCandidates(habits, habitLogs, tasks, taskHistory, dailyStates, recentFrom, to, utcNow);
        var newRecommendations = new List<Recommendation>();

        foreach (var candidate in candidates)
        {
            var exists = await recommendationRepository.ExistsForTodayAsync(
                userId,
                candidate.Type,
                candidate.Message,
                utcNow,
                cancellationToken);

            if (exists)
            {
                continue;
            }

            newRecommendations.Add(new Recommendation
            {
                UserId = userId,
                Type = candidate.Type,
                Title = candidate.Title,
                Message = candidate.Message,
                Priority = candidate.Priority,
                IsRead = false,
                CreatedAt = utcNow,
                ExpiresAt = candidate.ExpiresAt
            });
        }

        if (newRecommendations.Count > 0)
        {
            await recommendationRepository.AddRangeAsync(newRecommendations, cancellationToken);
            await recommendationRepository.SaveChangesAsync(cancellationToken);
        }

        return Result<IReadOnlyList<RecommendationResponse>>.Success(newRecommendations.Select(ToResponse).ToList());
    }

    public async Task<Result> DeleteAsync(int userId, int recommendationId, CancellationToken cancellationToken = default)
    {
        var recommendationResult = await GetOwnedRecommendationAsync(userId, recommendationId, cancellationToken);
        if (!recommendationResult.Succeeded)
        {
            return Result.Failure(recommendationResult.Error!, recommendationResult.ErrorType);
        }

        recommendationRepository.Remove(recommendationResult.Value!);
        await recommendationRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<RecommendationResponse>> MarkAsReadAsync(int userId, int recommendationId, CancellationToken cancellationToken = default)
    {
        var recommendationResult = await GetOwnedRecommendationAsync(userId, recommendationId, cancellationToken);
        if (!recommendationResult.Succeeded)
        {
            return Result<RecommendationResponse>.Failure(recommendationResult.Error!, recommendationResult.ErrorType);
        }

        var recommendation = recommendationResult.Value!;
        recommendation.IsRead = true;

        recommendationRepository.Update(recommendation);
        await recommendationRepository.SaveChangesAsync(cancellationToken);

        return Result<RecommendationResponse>.Success(ToResponse(recommendation));
    }

    private async Task<Result<Recommendation>> GetOwnedRecommendationAsync(int userId, int recommendationId, CancellationToken cancellationToken)
    {
        var recommendation = await recommendationRepository.GetByIdAsync(recommendationId, cancellationToken);
        if (recommendation is null)
        {
            return Result<Recommendation>.Failure("Recommendation was not found.", ErrorType.NotFound);
        }

        if (recommendation.UserId != userId)
        {
            return Result<Recommendation>.Failure("You do not have permission to access this recommendation.", ErrorType.Forbidden);
        }

        return Result<Recommendation>.Success(recommendation);
    }

    private static IReadOnlyList<RecommendationCandidate> BuildCandidates(
        IReadOnlyList<Habit> habits,
        IReadOnlyList<HabitLog> habitLogs,
        IReadOnlyList<UserTask> tasks,
        IReadOnlyList<TaskHistory> taskHistory,
        IReadOnlyList<DailyState> dailyStates,
        DateOnly recentFrom,
        DateOnly to,
        DateTime utcNow)
    {
        var candidates = new List<RecommendationCandidate>();
        var activeHabits = habits.Where(x => x.IsActive).ToList();
        var recentStates = dailyStates.Where(x => x.Date >= recentFrom && x.Date <= to).ToList();
        var expiresAt = utcNow.AddDays(14);

        AddBurnoutRecommendation(candidates, recentStates, expiresAt);
        AddSleepRecommendation(candidates, dailyStates, expiresAt);
        AddProductivityOverloadRecommendation(candidates, tasks, expiresAt);
        AddHabitStreakRecommendation(candidates, habits, habitLogs, expiresAt);
        AddMissingHabitsRecommendation(candidates, activeHabits, expiresAt);
        AddWellnessImbalanceRecommendation(candidates, dailyStates, expiresAt);
        AddHabitConsistencyRecommendation(candidates, habitLogs, expiresAt);
        AddTaskPlanningRecommendation(candidates, tasks, taskHistory, expiresAt);

        return candidates;
    }

    private static void AddBurnoutRecommendation(List<RecommendationCandidate> candidates, IReadOnlyList<DailyState> recentStates, DateTime expiresAt)
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
            candidates.Add(new RecommendationCandidate(
                RecommendationTypes.Burnout,
                "Risk of burnout detected",
                $"Your last 7 days show high stress ({averageStress}), low energy ({averageEnergy}), and low sleep quality ({averageSleepQuality}). Consider reducing workload, adding recovery time, and protecting sleep for the next few days.",
                5,
                expiresAt));
        }
    }

    private static void AddSleepRecommendation(List<RecommendationCandidate> candidates, IReadOnlyList<DailyState> dailyStates, DateTime expiresAt)
    {
        var sleepDurations = dailyStates
            .Where(x => x.SleepDuration.HasValue)
            .Select(x => x.SleepDuration!.Value)
            .ToList();

        if (sleepDurations.Count == 0)
        {
            return;
        }

        var averageSleepDuration = Round(sleepDurations.Average());
        if (averageSleepDuration < 6m)
        {
            candidates.Add(new RecommendationCandidate(
                RecommendationTypes.Sleep,
                "Sleep duration is below target",
                $"Your average sleep duration over the analyzed period is {averageSleepDuration} hours. Try moving demanding tasks away from late evening and create a calmer sleep routine.",
                4,
                expiresAt));
        }
    }

    private static void AddProductivityOverloadRecommendation(List<RecommendationCandidate> candidates, IReadOnlyList<UserTask> tasks, DateTime expiresAt)
    {
        if (tasks.Count == 0)
        {
            return;
        }

        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var overdueTasks = CountOverdueTasks(tasks, currentDate);
        var averageDifficultyLoad = Round(tasks.GroupBy(x => x.PlannedDate).Average(x => (decimal)x.Sum(task => task.Difficulty)));

        if (overdueTasks >= 3 || averageDifficultyLoad >= 8m)
        {
            candidates.Add(new RecommendationCandidate(
                RecommendationTypes.Productivity,
                "Workload may be too heavy",
                $"You have {overdueTasks} overdue tasks and an average planned difficulty load of {averageDifficultyLoad} points per active day. Reduce today's scope or move lower-priority work to protect execution quality.",
                4,
                expiresAt));
        }
    }

    private static void AddHabitStreakRecommendation(List<RecommendationCandidate> candidates, IReadOnlyList<Habit> habits, IReadOnlyList<HabitLog> habitLogs, DateTime expiresAt)
    {
        var bestStreak = habits
            .Select(habit => new
            {
                Habit = habit,
                Streak = CalculateCurrentStreak(habitLogs
                    .Where(x => x.HabitId == habit.Id && x.Status == HabitLogStatuses.Completed)
                    .Select(x => x.Date)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList())
            })
            .OrderByDescending(x => x.Streak)
            .ThenBy(x => x.Habit.Name)
            .FirstOrDefault();

        if (bestStreak is not null && bestStreak.Streak >= 7)
        {
            candidates.Add(new RecommendationCandidate(
                RecommendationTypes.Motivation,
                "Strong habit streak",
                $"Great consistency: your habit \"{bestStreak.Habit.Name}\" has a {bestStreak.Streak}-day completion streak. Keep the habit small and repeatable to preserve the momentum.",
                2,
                expiresAt));
        }
    }

    private static void AddMissingHabitsRecommendation(List<RecommendationCandidate> candidates, IReadOnlyList<Habit> activeHabits, DateTime expiresAt)
    {
        if (activeHabits.Count > 0)
        {
            return;
        }

        candidates.Add(new RecommendationCandidate(
            RecommendationTypes.Habit,
            "Create a small active habit",
            "You do not have active habits yet. Start with one simple daily habit, such as a 10-minute walk or planning tomorrow's top task.",
            3,
            expiresAt));
    }

    private static void AddWellnessImbalanceRecommendation(List<RecommendationCandidate> candidates, IReadOnlyList<DailyState> dailyStates, DateTime expiresAt)
    {
        if (dailyStates.Count == 0)
        {
            return;
        }

        var averageScreenTime = AverageOrZero(dailyStates.Where(x => x.ScreenTime.HasValue).Select(x => x.ScreenTime!.Value));
        var averageActivityMinutes = AverageOrZero(dailyStates.Where(x => x.ActivityDuration.HasValue).Select(x => x.ActivityDuration!.Value));

        if (averageScreenTime >= 5m && averageActivityMinutes < 20m)
        {
            candidates.Add(new RecommendationCandidate(
                RecommendationTypes.Wellness,
                "Balance screen time with movement",
                $"Your average screen time is {averageScreenTime} hours while activity averages {averageActivityMinutes} minutes. Add a short walk or stretching block after long screen sessions.",
                3,
                expiresAt));
        }
    }

    private static void AddHabitConsistencyRecommendation(List<RecommendationCandidate> candidates, IReadOnlyList<HabitLog> habitLogs, DateTime expiresAt)
    {
        if (habitLogs.Count < 3)
        {
            return;
        }

        var completedLogs = habitLogs.Count(x => x.Status == HabitLogStatuses.Completed);
        var completionRate = Percentage(completedLogs, habitLogs.Count);

        if (completionRate < 50m)
        {
            candidates.Add(new RecommendationCandidate(
                RecommendationTypes.Habit,
                "Habit consistency needs attention",
                $"Your habit completion rate is {completionRate}% for the analyzed period. Lower the difficulty or frequency of one habit so completion becomes easier to sustain.",
                3,
                expiresAt));
        }
    }

    private static void AddTaskPlanningRecommendation(List<RecommendationCandidate> candidates, IReadOnlyList<UserTask> tasks, IReadOnlyList<TaskHistory> taskHistory, DateTime expiresAt)
    {
        if (tasks.Count == 0)
        {
            return;
        }

        var reschedules = taskHistory.Count;
        var completionRate = Percentage(tasks.Count(x => x.Status == TaskStatuses.Completed), tasks.Count);

        if (reschedules >= 3 || completionRate < 40m)
        {
            candidates.Add(new RecommendationCandidate(
                RecommendationTypes.Task,
                "Review task planning",
                $"Your task completion rate is {completionRate}% and {reschedules} reschedules were recorded recently. Plan fewer high-difficulty tasks and reserve buffer time for changes.",
                4,
                expiresAt));
        }
    }

    private static int CalculateCurrentStreak(IReadOnlyList<DateOnly> orderedDates)
    {
        if (orderedDates.Count == 0)
        {
            return 0;
        }

        var streak = 1;
        var previous = orderedDates[^1];

        for (var i = orderedDates.Count - 2; i >= 0; i--)
        {
            if (orderedDates[i].AddDays(1) != previous)
            {
                break;
            }

            streak++;
            previous = orderedDates[i];
        }

        return streak;
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

    private static decimal Percentage(int numerator, int denominator)
    {
        return denominator == 0 ? 0m : Round(numerator * 100m / denominator);
    }

    private static decimal AverageOrZero(IEnumerable<decimal> values)
    {
        var list = values.ToList();
        return list.Count == 0 ? 0m : Round(list.Average());
    }

    private static decimal Round(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    private static RecommendationResponse ToResponse(Recommendation recommendation)
    {
        return new RecommendationResponse(
            recommendation.Id,
            recommendation.Type,
            recommendation.Title,
            recommendation.Message,
            recommendation.Priority,
            recommendation.IsRead,
            recommendation.CreatedAt,
            recommendation.ExpiresAt);
    }

    private sealed record RecommendationCandidate(
        string Type,
        string Title,
        string Message,
        int Priority,
        DateTime? ExpiresAt);
}
