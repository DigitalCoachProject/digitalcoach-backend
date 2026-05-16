using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Application.Abstractions.Services;
using DigitalCoach.Application.Common;
using DigitalCoach.Application.DTOs.Analytics;
using DigitalCoach.Domain.Constants;
using DigitalCoach.Domain.Entities;
using DigitalCoach.Domain.Views;

namespace DigitalCoach.Infrastructure.Services;

public sealed class AnalyticsService(IAnalyticsRepository analyticsRepository) : IAnalyticsService
{
    private const decimal NeutralIndex = 0.5m;
    private const decimal LoadMax = 20m;
    private const decimal OverloadThreshold = 0.7m;
    private const decimal OverloadImpact = 0.3m;

    public async Task<Result<DashboardAnalyticsResponse>> GetDashboardAsync(int userId, AnalyticsRangeRequest request, CancellationToken cancellationToken = default)
    {
        var range = NormalizeRange(request);
        if (!range.Succeeded)
        {
            return Result<DashboardAnalyticsResponse>.Failure(range.Error!, range.ErrorType);
        }

        var data = await LoadAnalyticsDataAsync(userId, range.Value!, cancellationToken);
        var productivityScores = CalculateProductivityByDay(data, range.Value!);

        var totalHabitLogs = data.HabitLogs.Count;
        var completedHabitLogs = data.HabitLogs.Count(x => x.Status == HabitLogStatuses.Completed);
        var totalTasks = data.Tasks.Count;
        var completedTasks = data.Tasks.Count(x => x.Status == TaskStatuses.Completed);

        var response = new DashboardAnalyticsResponse(
            data.Habits.Count,
            data.Habits.Count(x => x.IsActive),
            completedHabitLogs,
            Percentage(completedHabitLogs, totalHabitLogs),
            totalTasks,
            completedTasks,
            data.Tasks.Count(x => x.Status == TaskStatuses.Cancelled),
            CountOverdueTasks(data.Tasks, DateOnly.FromDateTime(DateTime.UtcNow)),
            Percentage(completedTasks, totalTasks),
            AverageOrZero(data.DailyStates.Select(x => (decimal)x.Mood)),
            AverageOrZero(data.DailyStates.Select(x => (decimal)x.Stress)),
            AverageOrZero(data.DailyStates.Select(x => (decimal)x.Energy)),
            AverageOrZero(data.DailyStates.Where(x => x.SleepQuality.HasValue).Select(x => (decimal)x.SleepQuality!.Value)),
            AverageOrZero(data.DailyStates.Select(x => (decimal)x.PhysicalState)),
            Round(data.DailyStates.Sum(x => x.ActivityDuration ?? 0m)),
            AverageOrZero(data.DailyStates.Where(x => x.ScreenTime.HasValue).Select(x => x.ScreenTime!.Value)),
            AverageOrZero(productivityScores.Select(x => x.ScorePercent)));

        return Result<DashboardAnalyticsResponse>.Success(response);
    }

    public async Task<Result<ProductivityAnalyticsResponse>> GetProductivityAsync(int userId, AnalyticsRangeRequest request, CancellationToken cancellationToken = default)
    {
        var range = NormalizeRange(request);
        if (!range.Succeeded)
        {
            return Result<ProductivityAnalyticsResponse>.Failure(range.Error!, range.ErrorType);
        }

        var data = await LoadAnalyticsDataAsync(userId, range.Value!, cancellationToken);
        var productivityScores = CalculateProductivityByDay(data, range.Value!);
        var productivityByDate = productivityScores.ToDictionary(x => x.Date, x => x.ScorePercent);

        var trend = data.ProductivityOverview
            .Select(x => new ProductivityTrendResponse(
                x.Date,
                productivityByDate.TryGetValue(x.Date, out var score) ? score : 0m,
                x.TasksCount,
                x.CompletedTasks,
                x.HabitLogs,
                x.CompletedHabits))
            .ToList();

        var mostProductiveDayType = data.DailyStates
            .Where(x => !string.IsNullOrWhiteSpace(x.DayType) && productivityByDate.ContainsKey(x.Date))
            .GroupBy(x => x.DayType!)
            .Select(x => new
            {
                DayType = x.Key,
                AverageScore = x.Average(day => productivityByDate[day.Date])
            })
            .OrderByDescending(x => x.AverageScore)
            .ThenBy(x => x.DayType)
            .Select(x => x.DayType)
            .FirstOrDefault();

        var response = new ProductivityAnalyticsResponse(
            ToDailyCounts(data.Tasks.Where(x => x.Status == TaskStatuses.Completed).GroupBy(x => x.PlannedDate)),
            ToDailyCounts(data.HabitLogs.Where(x => x.Status == HabitLogStatuses.Completed).GroupBy(x => x.Date)),
            trend,
            mostProductiveDayType,
            CountOverdueTasks(data.Tasks, DateOnly.FromDateTime(DateTime.UtcNow)),
            Round(data.Tasks.Count / (decimal)range.Value!.DaysCount));

        return Result<ProductivityAnalyticsResponse>.Success(response);
    }

    public async Task<Result<WellnessAnalyticsResponse>> GetWellnessAsync(int userId, AnalyticsRangeRequest request, CancellationToken cancellationToken = default)
    {
        var range = NormalizeRange(request);
        if (!range.Succeeded)
        {
            return Result<WellnessAnalyticsResponse>.Failure(range.Error!, range.ErrorType);
        }

        var dailyStates = await analyticsRepository.ListDailyStatesAsync(userId, range.Value!.From, range.Value.To, cancellationToken);
        var response = new WellnessAnalyticsResponse(
            ToDailyMetrics(dailyStates, x => x.Mood),
            ToDailyMetrics(dailyStates, x => x.Stress),
            ToDailyMetrics(dailyStates, x => x.Energy),
            dailyStates
                .Where(x => x.SleepQuality.HasValue)
                .Select(x => new DailyMetricResponse(x.Date, Round(x.SleepQuality!.Value)))
                .ToList(),
            ToDailyMetrics(dailyStates, x => x.PhysicalState),
            CalculateBurnoutRiskLevel(dailyStates));

        return Result<WellnessAnalyticsResponse>.Success(response);
    }

    public async Task<Result<HabitAnalyticsResponse>> GetHabitsAsync(int userId, AnalyticsRangeRequest request, CancellationToken cancellationToken = default)
    {
        var range = NormalizeRange(request);
        if (!range.Succeeded)
        {
            return Result<HabitAnalyticsResponse>.Failure(range.Error!, range.ErrorType);
        }

        var habits = await analyticsRepository.ListHabitsAsync(userId, range.Value!.To, cancellationToken);
        var logs = await analyticsRepository.ListHabitLogsAsync(userId, range.Value.From, range.Value.To, cancellationToken);

        var completedLogs = logs.Count(x => x.Status == HabitLogStatuses.Completed);
        var failedLogs = logs.Count(x => x.Status == HabitLogStatuses.Failed);
        var skippedLogs = logs.Count(x => x.Status == HabitLogStatuses.Skipped);
        var consistency = CalculateHabitConsistency(habits, logs);

        var response = new HabitAnalyticsResponse(
            habits.Count,
            habits.Count(x => x.IsActive),
            completedLogs,
            failedLogs,
            skippedLogs,
            Percentage(completedLogs, logs.Count),
            consistency.OrderByDescending(x => x.CompletionRate).ThenBy(x => x.HabitName).FirstOrDefault(),
            consistency.OrderBy(x => x.CompletionRate).ThenBy(x => x.HabitName).FirstOrDefault(),
            CalculateHabitStreaks(habits, logs, current: true),
            CalculateHabitStreaks(habits, logs, current: false));

        return Result<HabitAnalyticsResponse>.Success(response);
    }

    public async Task<Result<TaskAnalyticsResponse>> GetTasksAsync(int userId, AnalyticsRangeRequest request, CancellationToken cancellationToken = default)
    {
        var range = NormalizeRange(request);
        if (!range.Succeeded)
        {
            return Result<TaskAnalyticsResponse>.Failure(range.Error!, range.ErrorType);
        }

        var tasks = await analyticsRepository.ListTasksAsync(userId, range.Value!.From, range.Value.To, cancellationToken);
        var history = await analyticsRepository.ListTaskHistoryAsync(userId, range.Value.From, range.Value.To, cancellationToken);
        var completedTasks = tasks.Where(x => x.Status == TaskStatuses.Completed).ToList();
        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var mostRescheduledTask = history
            .GroupBy(x => new { x.TaskId, x.Task.Name })
            .Select(x => new TaskSummaryResponse(x.Key.TaskId, x.Key.Name, x.Count()))
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.TaskName)
            .FirstOrDefault();

        var completedTasksWithTime = completedTasks.Where(x => x.CompletedAt.HasValue).ToList();
        var averageCompletionTime = completedTasksWithTime.Count == 0
            ? 0m
            : Round(completedTasksWithTime.Average(x => (decimal)(x.CompletedAt!.Value - x.CreatedAt).TotalHours));

        var response = new TaskAnalyticsResponse(
            tasks.Count,
            completedTasks.Count,
            tasks.Count(x => x.Status == TaskStatuses.Cancelled),
            CountOverdueTasks(tasks, currentDate),
            tasks.Count(x => x.Status == TaskStatuses.Planned),
            Percentage(completedTasks.Count, tasks.Count),
            averageCompletionTime,
            mostRescheduledTask,
            ToDailyCounts(completedTasks.GroupBy(x => x.PlannedDate)));

        return Result<TaskAnalyticsResponse>.Success(response);
    }

    private async Task<AnalyticsData> LoadAnalyticsDataAsync(int userId, AnalyticsRange range, CancellationToken cancellationToken)
    {
        var habits = await analyticsRepository.ListHabitsAsync(userId, range.To, cancellationToken);
        var habitLogs = await analyticsRepository.ListHabitLogsAsync(userId, range.From, range.To, cancellationToken);
        var tasks = await analyticsRepository.ListTasksAsync(userId, range.From, range.To, cancellationToken);
        var dailyStates = await analyticsRepository.ListDailyStatesAsync(userId, range.From, range.To, cancellationToken);
        var productivityOverview = await analyticsRepository.ListProductivityOverviewAsync(userId, range.From, range.To, cancellationToken);

        return new AnalyticsData(habits, habitLogs, tasks, dailyStates, productivityOverview);
    }

    private static Result<AnalyticsRange> NormalizeRange(AnalyticsRangeRequest request)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var to = request.To ?? today;
        var from = request.From ?? to.AddDays(-29);

        if (from > to)
        {
            return Result<AnalyticsRange>.Failure("'from' must be earlier than or equal to 'to'.");
        }

        return Result<AnalyticsRange>.Success(new AnalyticsRange(from, to));
    }

    private static IReadOnlyList<DailyProductivityScore> CalculateProductivityByDay(AnalyticsData data, AnalyticsRange range)
    {
        var statesByDate = data.DailyStates.ToDictionary(x => x.Date);
        var tasksByDate = data.Tasks.GroupBy(x => x.PlannedDate).ToDictionary(x => x.Key, x => x.ToList());
        var logsByDate = data.HabitLogs.GroupBy(x => x.Date).ToDictionary(x => x.Key, x => x.ToList());
        var scores = new List<DailyProductivityScore>();

        foreach (var date in EachDate(range.From, range.To))
        {
            statesByDate.TryGetValue(date, out var state);
            tasksByDate.TryGetValue(date, out var tasks);
            logsByDate.TryGetValue(date, out var logs);

            if (state is null && (tasks is null || tasks.Count == 0) && (logs is null || logs.Count == 0))
            {
                continue;
            }

            var taskIndex = CalculateTaskIndex(tasks ?? []);
            var habitIndex = CalculateHabitIndex(logs ?? []);
            var stateIndex = state is null ? NeutralIndex : CalculateStateIndex(state);
            var loadIndex = CalculateLoadIndex(tasks ?? [], logs ?? []);
            var overload = loadIndex <= OverloadThreshold
                ? 0m
                : (loadIndex - OverloadThreshold) / (1m - OverloadThreshold);

            var productivityIndex = (0.4m * taskIndex) + (0.3m * habitIndex) + (0.3m * stateIndex);
            var finalIndex = Clamp01(productivityIndex * (1m - (OverloadImpact * overload)));

            scores.Add(new DailyProductivityScore(date, Round(finalIndex * 100m)));
        }

        return scores;
    }

    private static decimal CalculateTaskIndex(IReadOnlyList<UserTask> tasks)
    {
        var totalDifficulty = tasks.Sum(x => x.Difficulty);
        if (totalDifficulty == 0)
        {
            return NeutralIndex;
        }

        var completedDifficulty = tasks
            .Where(x => x.Status == TaskStatuses.Completed)
            .Sum(x => x.Difficulty);

        return completedDifficulty / (decimal)totalDifficulty;
    }

    private static decimal CalculateHabitIndex(IReadOnlyList<HabitLog> logs)
    {
        var totalDifficulty = logs.Sum(x => x.Habit.Difficulty);
        if (totalDifficulty == 0)
        {
            return NeutralIndex;
        }

        var completedDifficulty = logs
            .Where(x => x.Status == HabitLogStatuses.Completed)
            .Sum(x => x.Habit.Difficulty);

        return completedDifficulty / (decimal)totalDifficulty;
    }

    private static decimal CalculateStateIndex(DailyState state)
    {
        return (NormalizeFivePoint(state.Energy)
            + NormalizeFivePoint(state.Mood)
            + NormalizeFivePoint(state.PhysicalState)
            + (1m - NormalizeFivePoint(state.Stress))) / 4m;
    }

    private static decimal CalculateLoadIndex(IReadOnlyList<UserTask> tasks, IReadOnlyList<HabitLog> logs)
    {
        var load = tasks.Sum(x => x.Difficulty) + logs.Sum(x => x.Habit.Difficulty);
        return Math.Min(load / LoadMax, 1m);
    }

    private static IReadOnlyList<HabitConsistencyResponse> CalculateHabitConsistency(IReadOnlyList<Habit> habits, IReadOnlyList<HabitLog> logs)
    {
        return habits
            .Select(habit =>
            {
                var habitLogs = logs.Where(x => x.HabitId == habit.Id).ToList();
                var completed = habitLogs.Count(x => x.Status == HabitLogStatuses.Completed);
                return new HabitConsistencyResponse(habit.Id, habit.Name, Percentage(completed, habitLogs.Count));
            })
            .Where(x => x.CompletionRate > 0m || logs.Any(log => log.HabitId == x.HabitId))
            .ToList();
    }

    private static IReadOnlyList<HabitStreakResponse> CalculateHabitStreaks(IReadOnlyList<Habit> habits, IReadOnlyList<HabitLog> logs, bool current)
    {
        return habits
            .Select(habit =>
            {
                var dates = logs
                    .Where(x => x.HabitId == habit.Id && x.Status == HabitLogStatuses.Completed)
                    .Select(x => x.Date)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                var streak = current
                    ? CalculateCurrentStreak(dates)
                    : CalculateLongestStreak(dates);

                return new HabitStreakResponse(habit.Id, habit.Name, streak);
            })
            .Where(x => x.Streak > 0)
            .OrderByDescending(x => x.Streak)
            .ThenBy(x => x.HabitName)
            .ToList();
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

    private static int CalculateLongestStreak(IReadOnlyList<DateOnly> orderedDates)
    {
        if (orderedDates.Count == 0)
        {
            return 0;
        }

        var longest = 1;
        var current = 1;

        for (var i = 1; i < orderedDates.Count; i++)
        {
            if (orderedDates[i - 1].AddDays(1) == orderedDates[i])
            {
                current++;
                longest = Math.Max(longest, current);
            }
            else
            {
                current = 1;
            }
        }

        return longest;
    }

    private static string CalculateBurnoutRiskLevel(IReadOnlyList<DailyState> dailyStates)
    {
        if (dailyStates.Count == 0)
        {
            return "LOW";
        }

        var averageStress = AverageOrZero(dailyStates.Select(x => (decimal)x.Stress));
        var averageEnergy = AverageOrZero(dailyStates.Select(x => (decimal)x.Energy));
        var averageSleepQuality = AverageOrZero(dailyStates.Where(x => x.SleepQuality.HasValue).Select(x => (decimal)x.SleepQuality!.Value));
        var averageScreenTime = AverageOrZero(dailyStates.Where(x => x.ScreenTime.HasValue).Select(x => x.ScreenTime!.Value));

        if (averageStress >= 4m && (averageEnergy <= 2.5m || averageSleepQuality <= 2.5m || averageScreenTime >= 7m))
        {
            return "HIGH";
        }

        if (averageStress >= 3.5m || averageEnergy <= 3m || averageSleepQuality <= 3m || averageScreenTime >= 5m)
        {
            return "MEDIUM";
        }

        return "LOW";
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

    private static IReadOnlyList<DailyCountResponse> ToDailyCounts(IEnumerable<IGrouping<DateOnly, UserTask>> groups)
    {
        return groups
            .Select(x => new DailyCountResponse(x.Key, x.Count()))
            .OrderBy(x => x.Date)
            .ToList();
    }

    private static IReadOnlyList<DailyCountResponse> ToDailyCounts(IEnumerable<IGrouping<DateOnly, HabitLog>> groups)
    {
        return groups
            .Select(x => new DailyCountResponse(x.Key, x.Count()))
            .OrderBy(x => x.Date)
            .ToList();
    }

    private static IReadOnlyList<DailyMetricResponse> ToDailyMetrics(IReadOnlyList<DailyState> dailyStates, Func<DailyState, int> selector)
    {
        return dailyStates
            .Select(x => new DailyMetricResponse(x.Date, Round(selector(x))))
            .OrderBy(x => x.Date)
            .ToList();
    }

    private static IEnumerable<DateOnly> EachDate(DateOnly from, DateOnly to)
    {
        for (var date = from; date <= to; date = date.AddDays(1))
        {
            yield return date;
        }
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

    private static decimal NormalizeFivePoint(int value)
    {
        return Clamp01((value - 1m) / 4m);
    }

    private static decimal Clamp01(decimal value)
    {
        return Math.Min(Math.Max(value, 0m), 1m);
    }

    private static decimal Round(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    private sealed record AnalyticsRange(DateOnly From, DateOnly To)
    {
        public int DaysCount => To.DayNumber - From.DayNumber + 1;
    }

    private sealed record AnalyticsData(
        IReadOnlyList<Habit> Habits,
        IReadOnlyList<HabitLog> HabitLogs,
        IReadOnlyList<UserTask> Tasks,
        IReadOnlyList<DailyState> DailyStates,
        IReadOnlyList<ProductivityOverview> ProductivityOverview);

    private sealed record DailyProductivityScore(DateOnly Date, decimal ScorePercent);
}
