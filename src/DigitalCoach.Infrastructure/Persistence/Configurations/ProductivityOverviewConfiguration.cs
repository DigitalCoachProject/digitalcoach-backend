using DigitalCoach.Domain.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalCoach.Infrastructure.Persistence.Configurations;

public sealed class ProductivityOverviewConfiguration : IEntityTypeConfiguration<ProductivityOverview>
{
    public void Configure(EntityTypeBuilder<ProductivityOverview> builder)
    {
        builder.ToView("vw_ProductivityOverview");
        builder.HasNoKey();

        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.Date).HasColumnName("date").HasColumnType("date");
        builder.Property(x => x.Energy).HasColumnName("energy");
        builder.Property(x => x.Mood).HasColumnName("mood");
        builder.Property(x => x.Stress).HasColumnName("stress");
        builder.Property(x => x.PhysicalState).HasColumnName("physical_state");
        builder.Property(x => x.TasksCount).HasColumnName("tasks_count");
        builder.Property(x => x.CompletedTasks).HasColumnName("completed_tasks");
        builder.Property(x => x.HabitLogs).HasColumnName("habit_logs");
        builder.Property(x => x.CompletedHabits).HasColumnName("completed_habits");
    }
}
