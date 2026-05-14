using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DigitalCoach.Infrastructure.Repositories;

public class Repository<TEntity>(DigitalCoachDbContext dbContext) : IRepository<TEntity>
    where TEntity : class
{
    protected DigitalCoachDbContext DbContext { get; } = dbContext;
    protected DbSet<TEntity> DbSet => DbContext.Set<TEntity>();

    public virtual Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return DbSet.FindAsync([id], cancellationToken).AsTask();
    }

    public virtual async Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return DbSet.AddAsync(entity, cancellationToken).AsTask();
    }

    public void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }

    public void Remove(TEntity entity)
    {
        DbSet.Remove(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return DbContext.SaveChangesAsync(cancellationToken);
    }
}
