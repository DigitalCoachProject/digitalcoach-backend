using DigitalCoach.Domain.Constants;
using DigitalCoach.Domain.Entities;
using DigitalCoach.Domain.Views;
using Microsoft.EntityFrameworkCore;

namespace DigitalCoach.Infrastructure.Persistence;

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
        ConfigureUserProfile(modelBuilder);
        ConfigureHabit(modelBuilder);
        ConfigureHabitLog(modelBuilder);
        ConfigureTask(modelBuilder);
        ConfigureTaskHistory(modelBuilder);
        ConfigureDailyState(modelBuilder);
        ConfigureProductivityOverview(modelBuilder);
    }

    private static void ConfigureUserProfile(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("UserProfile");
            entity.HasKey(x => x.Id).HasName("PK_UserProfile");

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Gender).HasColumnName("gender").HasMaxLength(20);
            entity.Property(x => x.Height).HasColumnName("height").HasColumnType("decimal(5,2)");
            entity.Property(x => x.Weight).HasColumnName("weight").HasColumnType("decimal(5,2)");
            entity.Property(x => x.BirthDate).HasColumnName("birth_date").HasColumnType("date");
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(500).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("SYSDATETIME()");

            entity.HasIndex(x => x.Email).IsUnique().HasDatabaseName("UQ_UserProfile_email");
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_UserProfile_height", "[height] > 0");
                t.HasCheckConstraint("CK_UserProfile_weight", "[weight] > 0");
            });
        });
    }

    private static void ConfigureHabit(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Habit>(entity =>
        {
            entity.ToTable("Habit");
            entity.HasKey(x => x.Id).HasName("PK_Habit");

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(x => x.Type).HasColumnName("type").HasMaxLength(20).IsRequired();
            entity.Property(x => x.Frequency).HasColumnName("frequency");
            entity.Property(x => x.DaysOfWeek).HasColumnName("days_of_week").HasMaxLength(100);
            entity.Property(x => x.Difficulty).HasColumnName("difficulty");
            entity.Property(x => x.StartDate).HasColumnName("start_date").HasColumnType("date");
            entity.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("SYSDATETIME()");

            entity.HasIndex(x => x.UserId).HasDatabaseName("IX_Habit_user_id");
            entity.HasOne(x => x.UserProfile)
                .WithMany(x => x.Habits)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Habit_UserProfile");

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Habit_type", $"[type] IN ('{HabitTypes.Daily}', '{HabitTypes.Weekly}', '{HabitTypes.SpecificDays}')");
                t.HasCheckConstraint("CK_Habit_frequency", "[frequency] IS NULL OR [frequency] BETWEEN 1 AND 7");
                t.HasCheckConstraint("CK_Habit_difficulty", "[difficulty] BETWEEN 1 AND 5");
            });
        });
    }

    private static void ConfigureHabitLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HabitLog>(entity =>
        {
            entity.ToTable("HabitLog");
            entity.HasKey(x => x.Id).HasName("PK_HabitLog");

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.HabitId).HasColumnName("habit_id");
            entity.Property(x => x.Date).HasColumnName("date").HasColumnType("date");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
            entity.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(100);
            entity.Property(x => x.Comment).HasColumnName("comment").HasMaxLength(500);

            entity.HasIndex(x => x.Date).HasDatabaseName("IX_HabitLog_date");
            entity.HasIndex(x => new { x.HabitId, x.Date }).IsUnique().HasDatabaseName("UQ_HabitLog_habit_date");
            entity.HasOne(x => x.Habit)
                .WithMany(x => x.Logs)
                .HasForeignKey(x => x.HabitId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_HabitLog_Habit");

            entity.ToTable(t => t.HasCheckConstraint("CK_HabitLog_status", $"[status] IN ('{HabitLogStatuses.Completed}', '{HabitLogStatuses.Failed}', '{HabitLogStatuses.Skipped}')"));
        });
    }

    private static void ConfigureTask(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserTask>(entity =>
        {
            entity.ToTable("Task");
            entity.HasKey(x => x.Id).HasName("PK_Task");

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(x => x.PlannedDate).HasColumnName("planned_date").HasColumnType("date");
            entity.Property(x => x.Deadline).HasColumnName("deadline").HasColumnType("date");
            entity.Property(x => x.Priority).HasColumnName("priority");
            entity.Property(x => x.Difficulty).HasColumnName("difficulty");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue(TaskStatuses.Planned).IsRequired();
            entity.Property(x => x.CompletedAt).HasColumnName("completed_at");
            entity.Property(x => x.RescheduleCount).HasColumnName("reschedule_count").HasDefaultValue(0);
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("SYSDATETIME()");

            entity.HasIndex(x => x.UserId).HasDatabaseName("IX_Task_user_id");
            entity.HasIndex(x => x.Status).HasDatabaseName("IX_Task_status");
            entity.HasOne(x => x.UserProfile)
                .WithMany(x => x.Tasks)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Task_UserProfile");

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Task_priority", "[priority] BETWEEN 1 AND 5");
                t.HasCheckConstraint("CK_Task_difficulty", "[difficulty] BETWEEN 1 AND 5");
                t.HasCheckConstraint("CK_Task_status", $"[status] IN ('{TaskStatuses.Planned}', '{TaskStatuses.Completed}', '{TaskStatuses.Overdue}', '{TaskStatuses.Cancelled}')");
                t.HasCheckConstraint("CK_Task_reschedule_count", "[reschedule_count] >= 0");
            });
        });
    }

    private static void ConfigureTaskHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskHistory>(entity =>
        {
            entity.ToTable("TaskHistory");
            entity.HasKey(x => x.Id).HasName("PK_TaskHistory");

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TaskId).HasColumnName("task_id");
            entity.Property(x => x.ChangeDate).HasColumnName("change_date").HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(x => x.OldDate).HasColumnName("old_date").HasColumnType("date");
            entity.Property(x => x.NewDate).HasColumnName("new_date").HasColumnType("date");
            entity.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(100);

            entity.HasIndex(x => x.TaskId).HasDatabaseName("IX_TaskHistory_task_id");
            entity.HasOne(x => x.Task)
                .WithMany(x => x.History)
                .HasForeignKey(x => x.TaskId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TaskHistory_Task");
        });
    }

    private static void ConfigureDailyState(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DailyState>(entity =>
        {
            entity.ToTable("DailyState");
            entity.HasKey(x => x.Id).HasName("PK_DailyState");

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.Date).HasColumnName("date").HasColumnType("date");
            entity.Property(x => x.SleepDuration).HasColumnName("sleep_duration").HasColumnType("decimal(4,2)");
            entity.Property(x => x.SleepQuality).HasColumnName("sleep_quality");
            entity.Property(x => x.Energy).HasColumnName("energy");
            entity.Property(x => x.Mood).HasColumnName("mood");
            entity.Property(x => x.Stress).HasColumnName("stress");
            entity.Property(x => x.PhysicalState).HasColumnName("physical_state");
            entity.Property(x => x.HasIllness).HasColumnName("has_illness").HasDefaultValue(false);
            entity.Property(x => x.HasPainOrInjury).HasColumnName("has_pain_or_injury").HasDefaultValue(false);
            entity.Property(x => x.CaloriesIntake).HasColumnName("calories_intake");
            entity.Property(x => x.HadMeals).HasColumnName("had_meals");
            entity.Property(x => x.MealsCount).HasColumnName("meals_count");
            entity.Property(x => x.Overeating).HasColumnName("overeating");
            entity.Property(x => x.Undereating).HasColumnName("undereating");
            entity.Property(x => x.Activity).HasColumnName("activity").HasMaxLength(100);
            entity.Property(x => x.ActivityDuration).HasColumnName("activity_duration").HasColumnType("decimal(5,2)");
            entity.Property(x => x.RestTaken).HasColumnName("rest_taken");
            entity.Property(x => x.ScreenTime).HasColumnName("screen_time").HasColumnType("decimal(5,2)");
            entity.Property(x => x.ScreenBeforeSleep).HasColumnName("screen_before_sleep");
            entity.Property(x => x.DayType).HasColumnName("day_type").HasMaxLength(30);
            entity.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(1000);
            entity.Property(x => x.ActivityType).HasColumnName("activity_type").HasMaxLength(50);
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("SYSDATETIME()");

            entity.HasIndex(x => new { x.UserId, x.Date }).IsUnique().HasDatabaseName("UQ_DailyState_user_date");
            entity.HasIndex(x => new { x.UserId, x.Date }).HasDatabaseName("IX_DailyState_user_id_date");
            entity.HasOne(x => x.UserProfile)
                .WithMany(x => x.DailyStates)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DailyState_UserProfile");

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_DailyState_sleep_duration", "[sleep_duration] IS NULL OR [sleep_duration] >= 0");
                t.HasCheckConstraint("CK_DailyState_sleep_quality", "[sleep_quality] IS NULL OR [sleep_quality] BETWEEN 1 AND 5");
                t.HasCheckConstraint("CK_DailyState_energy", "[energy] BETWEEN 1 AND 5");
                t.HasCheckConstraint("CK_DailyState_mood", "[mood] BETWEEN 1 AND 5");
                t.HasCheckConstraint("CK_DailyState_stress", "[stress] BETWEEN 1 AND 5");
                t.HasCheckConstraint("CK_DailyState_physical_state", "[physical_state] BETWEEN 1 AND 5");
                t.HasCheckConstraint("CK_DailyState_calories_intake", "[calories_intake] IS NULL OR [calories_intake] >= 0");
                t.HasCheckConstraint("CK_DailyState_meals_count", "[meals_count] IS NULL OR [meals_count] >= 0");
                t.HasCheckConstraint("CK_DailyState_activity_duration", "[activity_duration] IS NULL OR [activity_duration] >= 0");
                t.HasCheckConstraint("CK_DailyState_screen_time", "[screen_time] IS NULL OR [screen_time] >= 0");
                t.HasCheckConstraint("CK_DailyState_activity_type", "[activity_type] IS NULL OR [activity_type] IN ('walking', 'running', 'gym', 'cycling', 'stretching', 'yoga', 'other')");
            });
        });
    }

    private static void ConfigureProductivityOverview(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductivityOverview>(entity =>
        {
            entity.ToView("vw_ProductivityOverview");
            entity.HasNoKey();

            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.Date).HasColumnName("date").HasColumnType("date");
            entity.Property(x => x.Energy).HasColumnName("energy");
            entity.Property(x => x.Mood).HasColumnName("mood");
            entity.Property(x => x.Stress).HasColumnName("stress");
            entity.Property(x => x.PhysicalState).HasColumnName("physical_state");
            entity.Property(x => x.TasksCount).HasColumnName("tasks_count");
            entity.Property(x => x.CompletedTasks).HasColumnName("completed_tasks");
            entity.Property(x => x.HabitLogs).HasColumnName("habit_logs");
            entity.Property(x => x.CompletedHabits).HasColumnName("completed_habits");
        });
    }
}
