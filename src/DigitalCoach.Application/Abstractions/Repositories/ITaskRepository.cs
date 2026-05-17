using DigitalCoach.Application.Common;
using DigitalCoach.Domain.Entities;

namespace DigitalCoach.Application.Abstractions.Repositories;

public interface ITaskRepository : IRepository<UserTask>
{
    Task<PaginatedResponse<UserTask>> ListByUserAsync(
        int userId,
        string? status,
        int? priority,
        DateOnly? from,
        DateOnly? to,
        bool? overdue,
        string? sortBy,
        bool sortDescending,
        DateOnly currentDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
