using DigitalCoach.Domain.Constants;
using DigitalCoach.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalCoach.Infrastructure.Persistence.Configurations;

public sealed class UserTaskConfiguration : IEntityTypeConfiguration<UserTask>
{
    public void Configure(EntityTypeBuilder<UserTask> builder)
    {
        builder.ToTable("Task", table =>
        {
            table.HasCheckConstraint("CK_Task_priority", "[priority] BETWEEN 1 AND 5");
            table.HasCheckConstraint("CK_Task_difficulty", "[difficulty] BETWEEN 1 AND 5");
            table.HasCheckConstraint("CK_Task_status", $"[status] IN ('{TaskStatuses.Planned}', '{TaskStatuses.Completed}', '{TaskStatuses.Overdue}', '{TaskStatuses.Cancelled}')");
            table.HasCheckConstraint("CK_Task_reschedule_count", "[reschedule_count] >= 0");
        });

        builder.HasKey(x => x.Id).HasName("PK_Task");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(x => x.PlannedDate)
            .HasColumnName("planned_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.Deadline)
            .HasColumnName("deadline")
            .HasColumnType("date");

        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .IsRequired();

        builder.Property(x => x.Difficulty)
            .HasColumnName("difficulty")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasDefaultValue(TaskStatuses.Planned)
            .IsRequired();

        builder.Property(x => x.CompletedAt)
            .HasColumnName("completed_at")
            .HasColumnType("datetime2");

        builder.Property(x => x.RescheduleCount)
            .HasColumnName("reschedule_count")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSDATETIME()")
            .IsRequired();

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_Task_user_id");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_Task_status");

        builder.HasOne(x => x.UserProfile)
            .WithMany(x => x.Tasks)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Task_UserProfile");
    }
}
