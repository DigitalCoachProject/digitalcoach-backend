using DigitalCoach.Domain.Entities;

namespace DigitalCoach.Application.Abstractions.Repositories;

public interface ITaskRepository : IRepository<UserTask>
{
    Task<IReadOnlyList<UserTask>> ListByUserAsync(
        int userId,
        string? status,
        int? priority,
        DateOnly? from,
        DateOnly? to,
        bool? overdue,
        string? sortBy,
        bool sortDescending,
        DateOnly currentDate,
        CancellationToken cancellationToken = default);
}
