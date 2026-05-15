using DigitalCoach.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalCoach.Infrastructure.Persistence.Configurations;

public sealed class TaskHistoryConfiguration : IEntityTypeConfiguration<TaskHistory>
{
    public void Configure(EntityTypeBuilder<TaskHistory> builder)
    {
        builder.ToTable("TaskHistory", table => table.UseSqlOutputClause(false));

        builder.HasKey(x => x.Id).HasName("PK_TaskHistory");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.TaskId)
            .HasColumnName("task_id")
            .IsRequired();

        builder.Property(x => x.ChangeDate)
            .HasColumnName("change_date")
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(x => x.OldDate)
            .HasColumnName("old_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.NewDate)
            .HasColumnName("new_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasColumnName("reason")
            .HasMaxLength(100);

        builder.HasIndex(x => x.TaskId)
            .HasDatabaseName("IX_TaskHistory_task_id");

        builder.HasOne(x => x.Task)
            .WithMany(x => x.History)
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_TaskHistory_Task");
    }
}
