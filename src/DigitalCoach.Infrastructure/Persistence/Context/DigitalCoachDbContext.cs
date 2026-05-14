using DigitalCoach.Domain.Entities;
using DigitalCoach.Domain.Views;
using Microsoft.EntityFrameworkCore;

namespace DigitalCoach.Infrastructure.Persistence.Context;

public sealed class DigitalCoachDbContext(DbContextOptions<DigitalCoachDbContext> options) : DbContext(options)
{
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Habit> Habits => Set<Habit>();
    public DbSet<HabitLog> HabitLogs => Set<HabitLog>();
    public DbSet<UserTask> Tasks => Set<UserTask>();
    public DbSet<TaskHistory> TaskHistories => Set<TaskHistory>();
    public DbSet<DailyState> DailyStates => Set<DailyState>();
    public DbSet<ProductivityOverview> ProductivityOverview => Set<ProductivityOverview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DigitalCoachDbContext).Assembly);
    }
}
