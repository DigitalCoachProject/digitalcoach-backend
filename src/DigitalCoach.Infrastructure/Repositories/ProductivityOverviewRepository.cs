using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Domain.Views;
using DigitalCoach.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DigitalCoach.Infrastructure.Repositories;

public sealed class ProductivityOverviewRepository(DigitalCoachDbContext dbContext) : IProductivityOverviewRepository
{
    public async Task<IReadOnlyList<ProductivityOverview>> ListByUserAsync(int userId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        var query = dbContext.ProductivityOverview.AsNoTracking().Where(x => x.UserId == userId);

        if (from.HasValue)
        {
            query = query.Where(x => x.Date >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.Date <= to.Value);
        }

        return await query.OrderBy(x => x.Date).ToListAsync(cancellationToken);
    }
}
