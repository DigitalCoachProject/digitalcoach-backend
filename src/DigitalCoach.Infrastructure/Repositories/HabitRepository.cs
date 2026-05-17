using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Application.Common;
using DigitalCoach.Domain.Entities;
using DigitalCoach.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DigitalCoach.Infrastructure.Repositories;

public sealed class HabitRepository(DigitalCoachDbContext dbContext)
    : Repository<Habit>(dbContext), IHabitRepository
{
    public async Task<PaginatedResponse<Habit>> ListByUserAsync(
        int userId,
        string? type,
        bool? isActive,
        DateOnly? startDateFrom,
        DateOnly? startDateTo,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbContext.Habits
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(x => x.Type == type);
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        if (startDateFrom.HasValue)
        {
            query = query.Where(x => x.StartDate >= startDateFrom.Value);
        }

        if (startDateTo.HasValue)
        {
            query = query.Where(x => x.StartDate <= startDateTo.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await ApplySorting(query, sortBy, sortDescending)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PaginatedResponse<Habit>.Create(items, page, pageSize, totalItems);
    }

    private static IQueryable<Habit> ApplySorting(IQueryable<Habit> query, string? sortBy, bool sortDescending)
    {
        return sortBy switch
        {
            "name" => sortDescending
                ? query.OrderByDescending(x => x.Name).ThenByDescending(x => x.Id)
                : query.OrderBy(x => x.Name).ThenBy(x => x.Id),
            "created_at" => sortDescending
                ? query.OrderByDescending(x => x.Id)
                : query.OrderBy(x => x.Id),
            "difficulty" => sortDescending
                ? query.OrderByDescending(x => x.Difficulty).ThenBy(x => x.Name)
                : query.OrderBy(x => x.Difficulty).ThenBy(x => x.Name),
            _ => query
                .OrderByDescending(x => x.IsActive)
                .ThenBy(x => x.StartDate)
                .ThenBy(x => x.Name)
        };
    }
}
