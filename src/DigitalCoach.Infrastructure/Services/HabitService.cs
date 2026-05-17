using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Application.Abstractions.Services;
using DigitalCoach.Application.Common;
using DigitalCoach.Application.DTOs.Habits;
using DigitalCoach.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DigitalCoach.Infrastructure.Services;

public sealed class HabitService(
    IHabitRepository habitRepository,
    IHabitLogRepository habitLogRepository) : IHabitService
{
    public async Task<Result<HabitResponse>> CreateAsync(int userId, CreateHabitRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var habit = new Habit
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Description = TrimToNull(request.Description),
            Type = request.Type,
            Frequency = request.Frequency,
            DaysOfWeek = TrimToNull(request.DaysOfWeek),
            Difficulty = request.Difficulty,
            StartDate = request.StartDate,
            IsActive = request.IsActive,
            UpdatedAt = now
        };

        await habitRepository.AddAsync(habit, cancellationToken);
        await habitRepository.SaveChangesAsync(cancellationToken);

        return Result<HabitResponse>.Success(ToResponse(habit));
    }

    public async Task<Result<PaginatedResponse<HabitResponse>>> ListAsync(int userId, HabitFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var page = await habitRepository.ListByUserAsync(
            userId,
            filter.Type,
            filter.IsActive,
            filter.StartDateFrom,
            filter.StartDateTo,
            filter.SortBy,
            filter.SortDescending,
            filter.Page,
            filter.PageSize,
            cancellationToken);

        var response = PaginatedResponse<HabitResponse>.Create(
            page.Items.Select(ToResponse).ToList(),
            page.Page,
            page.PageSize,
            page.TotalItems);

        return Result<PaginatedResponse<HabitResponse>>.Success(response);
    }

    public async Task<Result<HabitResponse>> GetByIdAsync(int userId, int habitId, CancellationToken cancellationToken = default)
    {
        var habitResult = await GetOwnedHabitAsync(userId, habitId, cancellationToken);
        return habitResult.Succeeded
            ? Result<HabitResponse>.Success(ToResponse(habitResult.Value!))
            : Result<HabitResponse>.Failure(habitResult.Error!, habitResult.ErrorType);
    }

    public async Task<Result<HabitResponse>> UpdateAsync(int userId, int habitId, UpdateHabitRequest request, CancellationToken cancellationToken = default)
    {
        var habitResult = await GetOwnedHabitAsync(userId, habitId, cancellationToken);
        if (!habitResult.Succeeded)
        {
            return Result<HabitResponse>.Failure(habitResult.Error!, habitResult.ErrorType);
        }

        var habit = habitResult.Value!;
        habit.Name = request.Name.Trim();
        habit.Description = TrimToNull(request.Description);
        habit.Type = request.Type;
        habit.Frequency = request.Frequency;
        habit.DaysOfWeek = TrimToNull(request.DaysOfWeek);
        habit.Difficulty = request.Difficulty;
        habit.StartDate = request.StartDate;
        habit.IsActive = request.IsActive;
        habit.UpdatedAt = DateTime.UtcNow;

        habitRepository.Update(habit);
        await habitRepository.SaveChangesAsync(cancellationToken);

        return Result<HabitResponse>.Success(ToResponse(habit));
    }

    public async Task<Result> DeleteAsync(int userId, int habitId, CancellationToken cancellationToken = default)
    {
        var habitResult = await GetOwnedHabitAsync(userId, habitId, cancellationToken);
        if (!habitResult.Succeeded)
        {
            return Result.Failure(habitResult.Error!, habitResult.ErrorType);
        }

        habitRepository.Remove(habitResult.Value!);
        await habitRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<HabitLogResponse>> CreateLogAsync(int userId, int habitId, CreateHabitLogRequest request, CancellationToken cancellationToken = default)
    {
        var habitResult = await GetOwnedHabitAsync(userId, habitId, cancellationToken);
        if (!habitResult.Succeeded)
        {
            return Result<HabitLogResponse>.Failure(habitResult.Error!, habitResult.ErrorType);
        }

        var existingLog = await habitLogRepository.GetByHabitAndDateAsync(habitId, request.Date, cancellationToken);
        if (existingLog is not null)
        {
            return Result<HabitLogResponse>.Failure("A log already exists for this habit and date.", ErrorType.Conflict);
        }

        var log = new HabitLog
        {
            HabitId = habitId,
            Date = request.Date,
            Status = request.Status,
            Reason = TrimToNull(request.Reason),
            Comment = TrimToNull(request.Comment)
        };

        await habitLogRepository.AddAsync(log, cancellationToken);

        try
        {
            await habitLogRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            return Result<HabitLogResponse>.Failure("A log already exists for this habit and date.", ErrorType.Conflict);
        }

        return Result<HabitLogResponse>.Success(ToResponse(log));
    }

    public async Task<Result<IReadOnlyList<HabitLogResponse>>> ListLogsAsync(int userId, int habitId, HabitLogFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var habitResult = await GetOwnedHabitAsync(userId, habitId, cancellationToken);
        if (!habitResult.Succeeded)
        {
            return Result<IReadOnlyList<HabitLogResponse>>.Failure(habitResult.Error!, habitResult.ErrorType);
        }

        var logs = await habitLogRepository.ListByHabitAsync(habitId, filter.From, filter.To, cancellationToken);
        return Result<IReadOnlyList<HabitLogResponse>>.Success(logs.Select(ToResponse).ToList());
    }

    private async Task<Result<Habit>> GetOwnedHabitAsync(int userId, int habitId, CancellationToken cancellationToken)
    {
        var habit = await habitRepository.GetByIdAsync(habitId, cancellationToken);
        if (habit is null)
        {
            return Result<Habit>.Failure("Habit was not found.", ErrorType.NotFound);
        }

        if (habit.UserId != userId)
        {
            return Result<Habit>.Failure("You do not have permission to access this habit.", ErrorType.Forbidden);
        }

        return Result<Habit>.Success(habit);
    }

    private static HabitResponse ToResponse(Habit habit)
    {
        return new HabitResponse(
            habit.Id,
            habit.UserId,
            habit.Name,
            habit.Description,
            habit.Type,
            habit.Frequency,
            habit.DaysOfWeek,
            habit.Difficulty,
            habit.StartDate,
            habit.IsActive,
            habit.UpdatedAt);
    }

    private static HabitLogResponse ToResponse(HabitLog log)
    {
        return new HabitLogResponse(
            log.Id,
            log.HabitId,
            log.Date,
            log.Status,
            log.Reason,
            log.Comment);
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
