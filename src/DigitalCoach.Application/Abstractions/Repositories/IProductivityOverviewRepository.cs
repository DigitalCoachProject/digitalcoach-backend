using DigitalCoach.Domain.Views;

namespace DigitalCoach.Application.Abstractions.Repositories;

public interface IProductivityOverviewRepository
{
    Task<IReadOnlyList<ProductivityOverview>> ListByUserAsync(int userId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);
}
