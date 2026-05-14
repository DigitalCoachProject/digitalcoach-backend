using DigitalCoach.Domain.Constants;
using DigitalCoach.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalCoach.Infrastructure.Persistence.Configurations;

public sealed class HabitLogConfiguration : IEntityTypeConfiguration<HabitLog>
{
    public void Configure(EntityTypeBuilder<HabitLog> builder)
    {
        builder.ToTable("HabitLog", table =>
        {
            table.HasCheckConstraint("CK_HabitLog_status", $"[status] IN ('{HabitLogStatuses.Completed}', '{HabitLogStatuses.Failed}', '{HabitLogStatuses.Skipped}')");
        });

        builder.HasKey(x => x.Id).HasName("PK_HabitLog");
        builder.HasAlternateKey(x => new { x.HabitId, x.Date }).HasName("UQ_HabitLog_habit_date");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.HabitId)
            .HasColumnName("habit_id")
            .IsRequired();

        builder.Property(x => x.Date)
            .HasColumnName("date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasColumnName("reason")
            .HasMaxLength(100);

        builder.Property(x => x.Comment)
            .HasColumnName("comment")
            .HasMaxLength(500);

        builder.HasIndex(x => x.Date)
            .HasDatabaseName("IX_HabitLog_date");

        builder.HasOne(x => x.Habit)
            .WithMany(x => x.Logs)
            .HasForeignKey(x => x.HabitId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_HabitLog_Habit");
    }
}
