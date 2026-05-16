using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Application.Abstractions.Services;
using DigitalCoach.Application.Common;
using DigitalCoach.Application.DTOs.Analytics;
using DigitalCoach.Application.DTOs.DailyStates;
using DigitalCoach.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DigitalCoach.Infrastructure.Services;

public sealed class DailyStateService(
    IDailyStateRepository dailyStateRepository,
    IProductivityOverviewRepository productivityOverviewRepository) : IDailyStateService
{
    public async Task<Result<DailyStateResponse>> CreateAsync(int userId, CreateDailyStateRequest request, CancellationToken cancellationToken = default)
    {
        var existingState = await dailyStateRepository.GetByUserAndDateAsync(userId, request.Date, cancellationToken);
        if (existingState is not null)
        {
            return Result<DailyStateResponse>.Failure("A daily state already exists for this date.", ErrorType.Conflict);
        }

        var state = new DailyState
        {
            UserId = userId,
            Date = request.Date,
            SleepDuration = request.SleepDuration,
            SleepQuality = request.SleepQuality,
            Energy = request.Energy,
            Mood = request.Mood,
            Stress = request.Stress,
            PhysicalState = request.PhysicalState,
            HasIllness = request.HasIllness,
            HasPainOrInjury = request.HasPainOrInjury,
            CaloriesIntake = request.CaloriesIntake,
            HadMeals = request.HadMeals,
            MealsCount = request.MealsCount,
            Overeating = request.Overeating,
            Undereating = request.Undereating,
            Activity = TrimToNull(request.Activity),
            ActivityDuration = request.ActivityDuration,
            RestTaken = request.RestTaken,
            ScreenTime = request.ScreenTime,
            ScreenBeforeSleep = request.ScreenBeforeSleep,
            DayType = TrimToNull(request.DayType),
            Notes = TrimToNull(request.Notes),
            ActivityType = TrimToNull(request.ActivityType),
            UpdatedAt = DateTime.UtcNow
        };

        await dailyStateRepository.AddAsync(state, cancellationToken);

        try
        {
            await dailyStateRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            return Result<DailyStateResponse>.Failure("A daily state already exists for this date.", ErrorType.Conflict);
        }

        return Result<DailyStateResponse>.Success(ToResponse(state));
    }

    public async Task<Result<IReadOnlyList<DailyStateResponse>>> ListAsync(int userId, DailyStateFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var states = await dailyStateRepository.ListByUserAsync(
            userId,
            filter.From,
            filter.To,
            filter.Mood,
            filter.Stress,
            filter.Energy,
            filter.PhysicalState,
            filter.HasIllness,
            filter.HasPainOrInjury,
            filter.ActivityType,
            cancellationToken);

        return Result<IReadOnlyList<DailyStateResponse>>.Success(states.Select(ToResponse).ToList());
    }

    public async Task<Result<DailyStateResponse>> GetByIdAsync(int userId, int dailyStateId, CancellationToken cancellationToken = default)
    {
        var stateResult = await GetOwnedDailyStateAsync(userId, dailyStateId, cancellationToken);
        return stateResult.Succeeded
            ? Result<DailyStateResponse>.Success(ToResponse(stateResult.Value!))
            : Result<DailyStateResponse>.Failure(stateResult.Error!, stateResult.ErrorType);
    }

    public async Task<Result<DailyStateResponse>> UpdateAsync(int userId, int dailyStateId, UpdateDailyStateRequest request, CancellationToken cancellationToken = default)
    {
        var stateResult = await GetOwnedDailyStateAsync(userId, dailyStateId, cancellationToken);
        if (!stateResult.Succeeded)
        {
            return Result<DailyStateResponse>.Failure(stateResult.Error!, stateResult.ErrorType);
        }

        var state = stateResult.Value!;
        if (state.Date != request.Date)
        {
            var existingState = await dailyStateRepository.GetByUserAndDateAsync(userId, request.Date, cancellationToken);
            if (existingState is not null && existingState.Id != dailyStateId)
            {
                return Result<DailyStateResponse>.Failure("A daily state already exists for this date.", ErrorType.Conflict);
            }
        }

        state.Date = request.Date;
        state.SleepDuration = request.SleepDuration;
        state.SleepQuality = request.SleepQuality;
        state.Energy = request.Energy;
        state.Mood = request.Mood;
        state.Stress = request.Stress;
        state.PhysicalState = request.PhysicalState;
        state.HasIllness = request.HasIllness;
        state.HasPainOrInjury = request.HasPainOrInjury;
        state.CaloriesIntake = request.CaloriesIntake;
        state.HadMeals = request.HadMeals;
        state.MealsCount = request.MealsCount;
        state.Overeating = request.Overeating;
        state.Undereating = request.Undereating;
        state.Activity = TrimToNull(request.Activity);
        state.ActivityDuration = request.ActivityDuration;
        state.RestTaken = request.RestTaken;
        state.ScreenTime = request.ScreenTime;
        state.ScreenBeforeSleep = request.ScreenBeforeSleep;
        state.DayType = TrimToNull(request.DayType);
        state.Notes = TrimToNull(request.Notes);
        state.ActivityType = TrimToNull(request.ActivityType);
        state.UpdatedAt = DateTime.UtcNow;

        dailyStateRepository.Update(state);

        try
        {
            await dailyStateRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            return Result<DailyStateResponse>.Failure("A daily state already exists for this date.", ErrorType.Conflict);
        }

        return Result<DailyStateResponse>.Success(ToResponse(state));
    }

    public async Task<Result> DeleteAsync(int userId, int dailyStateId, CancellationToken cancellationToken = default)
    {
        var stateResult = await GetOwnedDailyStateAsync(userId, dailyStateId, cancellationToken);
        if (!stateResult.Succeeded)
        {
            return Result.Failure(stateResult.Error!, stateResult.ErrorType);
        }

        dailyStateRepository.Remove(stateResult.Value!);
        await dailyStateRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<DailyStateSummaryResponse>> GetSummaryAsync(int userId, DailyStateRangeRequest range, CancellationToken cancellationToken = default)
    {
        var states = await dailyStateRepository.ListByUserAsync(
            userId,
            range.From,
            range.To,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            cancellationToken);

        var totalEntries = states.Count;
        if (totalEntries == 0)
        {
            return Result<DailyStateSummaryResponse>.Success(new DailyStateSummaryResponse(null, null, null, null, null, null, 0, 0, 0));
        }

        var response = new DailyStateSummaryResponse(
            AverageMood: Average(states.Select(x => (decimal?)x.Mood)),
            AverageStress: Average(states.Select(x => (decimal?)x.Stress)),
            AverageEnergy: Average(states.Select(x => (decimal?)x.Energy)),
            AverageSleepQuality: Average(states.Select(x => x.SleepQuality.HasValue ? (decimal?)x.SleepQuality.Value : null)),
            AveragePhysicalState: Average(states.Select(x => (decimal?)x.PhysicalState)),
            AverageScreenTime: Average(states.Select(x => x.ScreenTime)),
            TotalActivityMinutes: states.Sum(x => x.ActivityDuration ?? 0),
            TotalRestDays: states.Count(x => x.RestTaken == true),
            TotalEntries: totalEntries);

        return Result<DailyStateSummaryResponse>.Success(response);
    }

    public async Task<Result<IReadOnlyList<ProductivityOverviewDto>>> GetProductivityOverviewAsync(int userId, DailyStateRangeRequest range, CancellationToken cancellationToken = default)
    {
        var rows = await productivityOverviewRepository.ListByUserAsync(userId, range.From, range.To, cancellationToken);

        var response = rows
            .Select(x => new ProductivityOverviewDto(
                x.UserId,
                x.Date,
                x.Energy,
                x.Mood,
                x.Stress,
                x.PhysicalState,
                x.TasksCount,
                x.CompletedTasks,
                x.HabitLogs,
                x.CompletedHabits))
            .ToList();

        return Result<IReadOnlyList<ProductivityOverviewDto>>.Success(response);
    }

    private async Task<Result<DailyState>> GetOwnedDailyStateAsync(int userId, int dailyStateId, CancellationToken cancellationToken)
    {
        var state = await dailyStateRepository.GetByIdAsync(dailyStateId, cancellationToken);
        if (state is null)
        {
            return Result<DailyState>.Failure("Daily state was not found.", ErrorType.NotFound);
        }

        if (state.UserId != userId)
        {
            return Result<DailyState>.Failure("You do not have permission to access this daily state.", ErrorType.Forbidden);
        }

        return Result<DailyState>.Success(state);
    }

    private static DailyStateResponse ToResponse(DailyState state)
    {
        return new DailyStateResponse(
            state.Id,
            state.UserId,
            state.Date,
            state.SleepDuration,
            state.SleepQuality,
            state.Energy,
            state.Mood,
            state.Stress,
            state.PhysicalState,
            state.HasIllness,
            state.HasPainOrInjury,
            state.CaloriesIntake,
            state.HadMeals,
            state.MealsCount,
            state.Overeating,
            state.Undereating,
            state.Activity,
            state.ActivityDuration,
            state.RestTaken,
            state.ScreenTime,
            state.ScreenBeforeSleep,
            state.DayType,
            state.Notes,
            state.ActivityType,
            state.UpdatedAt);
    }

    private static decimal? Average(IEnumerable<decimal?> values)
    {
        var presentValues = values.Where(x => x.HasValue).Select(x => x!.Value).ToList();
        return presentValues.Count == 0 ? null : Math.Round(presentValues.Average(), 2);
    }

    private static string? TrimToNull(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException
            && sqlException.Errors.Cast<SqlError>().Any(error => error.Number is 2601 or 2627);
    }
}
