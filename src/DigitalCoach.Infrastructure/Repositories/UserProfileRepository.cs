using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Domain.Entities;
using DigitalCoach.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DigitalCoach.Infrastructure.Repositories;

public sealed class UserProfileRepository(DigitalCoachDbContext dbContext)
    : Repository<UserProfile>(dbContext), IUserProfileRepository
{
    public Task<UserProfile?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return DbContext.UserProfiles.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }
}
