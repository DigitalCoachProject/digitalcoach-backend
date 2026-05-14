using DigitalCoach.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalCoach.Infrastructure.Persistence.Configurations;

public sealed class DailyStateConfiguration : IEntityTypeConfiguration<DailyState>
{
    public void Configure(EntityTypeBuilder<DailyState> builder)
    {
        builder.ToTable("DailyState", table =>
        {
            table.HasCheckConstraint("CK_DailyState_sleep_duration", "[sleep_duration] IS NULL OR [sleep_duration] >= 0");
            table.HasCheckConstraint("CK_DailyState_sleep_quality", "[sleep_quality] IS NULL OR [sleep_quality] BETWEEN 1 AND 5");
            table.HasCheckConstraint("CK_DailyState_energy", "[energy] BETWEEN 1 AND 5");
            table.HasCheckConstraint("CK_DailyState_mood", "[mood] BETWEEN 1 AND 5");
            table.HasCheckConstraint("CK_DailyState_stress", "[stress] BETWEEN 1 AND 5");
            table.HasCheckConstraint("CK_DailyState_physical_state", "[physical_state] BETWEEN 1 AND 5");
            table.HasCheckConstraint("CK_DailyState_calories_intake", "[calories_intake] IS NULL OR [calories_intake] >= 0");
            table.HasCheckConstraint("CK_DailyState_meals_count", "[meals_count] IS NULL OR [meals_count] >= 0");
            table.HasCheckConstraint("CK_DailyState_activity_duration", "[activity_duration] IS NULL OR [activity_duration] >= 0");
            table.HasCheckConstraint("CK_DailyState_screen_time", "[screen_time] IS NULL OR [screen_time] >= 0");
            table.HasCheckConstraint("CK_DailyState_activity_type", "[activity_type] IS NULL OR [activity_type] IN ('walking', 'running', 'gym', 'cycling', 'stretching', 'yoga', 'other')");
        });

        builder.HasKey(x => x.Id).HasName("PK_DailyState");
        builder.HasAlternateKey(x => new { x.UserId, x.Date }).HasName("UQ_DailyState_user_date");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.Date)
            .HasColumnName("date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.SleepDuration)
            .HasColumnName("sleep_duration")
            .HasColumnType("decimal(4,2)");

        builder.Property(x => x.SleepQuality)
            .HasColumnName("sleep_quality");

        builder.Property(x => x.Energy)
            .HasColumnName("energy")
            .IsRequired();

        builder.Property(x => x.Mood)
            .HasColumnName("mood")
            .IsRequired();

        builder.Property(x => x.Stress)
            .HasColumnName("stress")
            .IsRequired();

        builder.Property(x => x.PhysicalState)
            .HasColumnName("physical_state")
            .IsRequired();

        builder.Property(x => x.HasIllness)
            .HasColumnName("has_illness")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.HasPainOrInjury)
            .HasColumnName("has_pain_or_injury")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.CaloriesIntake)
            .HasColumnName("calories_intake");

        builder.Property(x => x.HadMeals)
            .HasColumnName("had_meals");

        builder.Property(x => x.MealsCount)
            .HasColumnName("meals_count");

        builder.Property(x => x.Overeating)
            .HasColumnName("overeating");

        builder.Property(x => x.Undereating)
            .HasColumnName("undereating");

        builder.Property(x => x.Activity)
            .HasColumnName("activity")
            .HasMaxLength(100);

        builder.Property(x => x.ActivityDuration)
            .HasColumnName("activity_duration")
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.RestTaken)
            .HasColumnName("rest_taken");

        builder.Property(x => x.ScreenTime)
            .HasColumnName("screen_time")
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.ScreenBeforeSleep)
            .HasColumnName("screen_before_sleep");

        builder.Property(x => x.DayType)
            .HasColumnName("day_type")
            .HasMaxLength(30);

        builder.Property(x => x.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        builder.Property(x => x.ActivityType)
            .HasColumnName("activity_type")
            .HasMaxLength(50);

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSDATETIME()")
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.Date })
            .HasDatabaseName("IX_DailyState_user_id_date");

        builder.HasOne(x => x.UserProfile)
            .WithMany(x => x.DailyStates)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_DailyState_UserProfile");
    }
}
