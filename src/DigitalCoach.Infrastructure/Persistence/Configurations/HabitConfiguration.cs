using DigitalCoach.Domain.Constants;
using DigitalCoach.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalCoach.Infrastructure.Persistence.Configurations;

public sealed class HabitConfiguration : IEntityTypeConfiguration<Habit>
{
    public void Configure(EntityTypeBuilder<Habit> builder)
    {
        builder.ToTable("Habit", table =>
        {
            table.HasCheckConstraint("CK_Habit_type", $"[type] IN ('{HabitTypes.Daily}', '{HabitTypes.Weekly}', '{HabitTypes.SpecificDays}')");
            table.HasCheckConstraint("CK_Habit_frequency", "[frequency] IS NULL OR [frequency] BETWEEN 1 AND 7");
            table.HasCheckConstraint("CK_Habit_difficulty", "[difficulty] BETWEEN 1 AND 5");
        });

        builder.HasKey(x => x.Id).HasName("PK_Habit");

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

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Frequency)
            .HasColumnName("frequency");

        builder.Property(x => x.DaysOfWeek)
            .HasColumnName("days_of_week")
            .HasMaxLength(100);

        builder.Property(x => x.Difficulty)
            .HasColumnName("difficulty")
            .IsRequired();

        builder.Property(x => x.StartDate)
            .HasColumnName("start_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSDATETIME()")
            .IsRequired();

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_Habit_user_id");

        builder.HasOne(x => x.UserProfile)
            .WithMany(x => x.Habits)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Habit_UserProfile");
    }
}
