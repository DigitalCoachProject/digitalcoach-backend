using DigitalCoach.Application.Common;
using DigitalCoach.Domain.Entities;

namespace DigitalCoach.Application.Abstractions.Repositories;

public interface IHabitRepository : IRepository<Habit>
{
    Task<PaginatedResponse<Habit>> ListByUserAsync(
        int userId,
        string? type,
        bool? isActive,
        DateOnly? startDateFrom,
        DateOnly? startDateTo,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
