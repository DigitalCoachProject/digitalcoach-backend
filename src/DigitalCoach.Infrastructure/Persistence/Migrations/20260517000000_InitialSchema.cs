using DigitalCoach.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalCoach.Infrastructure.Persistence.Migrations;

[Migration("20260517000000_InitialSchema")]
[DbContext(typeof(DigitalCoachDbContext))]
public partial class InitialSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
IF OBJECT_ID(N'dbo.UserProfile', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserProfile (
        id INT IDENTITY(1,1) NOT NULL,
        gender NVARCHAR(20) NULL,
        height DECIMAL(5,2) NOT NULL,
        weight DECIMAL(5,2) NOT NULL,
        birth_date DATE NOT NULL,
        email NVARCHAR(255) NOT NULL,
        password_hash NVARCHAR(500) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_UserProfile_created_at DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_UserProfile_updated_at DEFAULT SYSDATETIME(),
        CONSTRAINT PK_UserProfile PRIMARY KEY (id),
        CONSTRAINT UQ_UserProfile_email UNIQUE (email),
        CONSTRAINT CK_UserProfile_height CHECK (height > 0),
        CONSTRAINT CK_UserProfile_weight CHECK (weight > 0)
    );
END;
""");

        migrationBuilder.Sql("""
IF OBJECT_ID(N'dbo.Habit', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Habit (
        id INT IDENTITY(1,1) NOT NULL,
        user_id INT NOT NULL,
        name NVARCHAR(100) NOT NULL,
        description NVARCHAR(500) NULL,
        type NVARCHAR(20) NOT NULL,
        frequency INT NULL,
        days_of_week NVARCHAR(100) NULL,
        difficulty INT NOT NULL,
        start_date DATE NOT NULL,
        is_active BIT NOT NULL CONSTRAINT DF_Habit_is_active DEFAULT 1,
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_Habit_updated_at DEFAULT SYSDATETIME(),
        CONSTRAINT PK_Habit PRIMARY KEY (id),
        CONSTRAINT FK_Habit_UserProfile FOREIGN KEY (user_id) REFERENCES dbo.UserProfile(id) ON DELETE CASCADE,
        CONSTRAINT CK_Habit_type CHECK (type IN ('daily', 'weekly', 'specific_days')),
        CONSTRAINT CK_Habit_frequency CHECK (frequency IS NULL OR frequency BETWEEN 1 AND 7),
        CONSTRAINT CK_Habit_difficulty CHECK (difficulty BETWEEN 1 AND 5)
    );
END;
""");

        migrationBuilder.Sql("""
IF OBJECT_ID(N'dbo.HabitLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HabitLog (
        id INT IDENTITY(1,1) NOT NULL,
        habit_id INT NOT NULL,
        date DATE NOT NULL,
        status NVARCHAR(20) NOT NULL,
        reason NVARCHAR(100) NULL,
        comment NVARCHAR(500) NULL,
        CONSTRAINT PK_HabitLog PRIMARY KEY (id),
        CONSTRAINT FK_HabitLog_Habit FOREIGN KEY (habit_id) REFERENCES dbo.Habit(id) ON DELETE CASCADE,
        CONSTRAINT UQ_HabitLog_habit_date UNIQUE (habit_id, date),
        CONSTRAINT CK_HabitLog_status CHECK (status IN ('completed', 'failed', 'skipped'))
    );
END;
""");

        migrationBuilder.Sql("""
IF OBJECT_ID(N'dbo.Task', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.[Task] (
        id INT IDENTITY(1,1) NOT NULL,
        user_id INT NOT NULL,
        name NVARCHAR(100) NOT NULL,
        description NVARCHAR(500) NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_Task_created_at DEFAULT SYSUTCDATETIME(),
        planned_date DATE NOT NULL,
        deadline DATE NULL,
        priority INT NOT NULL,
        difficulty INT NOT NULL,
        status NVARCHAR(20) NOT NULL CONSTRAINT DF_Task_status DEFAULT 'planned',
        completed_at DATETIME2 NULL,
        reschedule_count INT NOT NULL CONSTRAINT DF_Task_reschedule_count DEFAULT 0,
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_Task_updated_at DEFAULT SYSDATETIME(),
        CONSTRAINT PK_Task PRIMARY KEY (id),
        CONSTRAINT FK_Task_UserProfile FOREIGN KEY (user_id) REFERENCES dbo.UserProfile(id) ON DELETE CASCADE,
        CONSTRAINT CK_Task_priority CHECK (priority BETWEEN 1 AND 5),
        CONSTRAINT CK_Task_difficulty CHECK (difficulty BETWEEN 1 AND 5),
        CONSTRAINT CK_Task_status CHECK (status IN ('planned', 'completed', 'overdue', 'cancelled')),
        CONSTRAINT CK_Task_reschedule_count CHECK (reschedule_count >= 0)
    );
END;
""");

        migrationBuilder.Sql("""
IF OBJECT_ID(N'dbo.TaskHistory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaskHistory (
        id INT IDENTITY(1,1) NOT NULL,
        task_id INT NOT NULL,
        change_date DATETIME2 NOT NULL CONSTRAINT DF_TaskHistory_change_date DEFAULT SYSUTCDATETIME(),
        old_date DATE NOT NULL,
        new_date DATE NOT NULL,
        reason NVARCHAR(100) NULL,
        CONSTRAINT PK_TaskHistory PRIMARY KEY (id),
        CONSTRAINT FK_TaskHistory_Task FOREIGN KEY (task_id) REFERENCES dbo.[Task](id) ON DELETE CASCADE
    );
END;
""");

        migrationBuilder.Sql("""
IF OBJECT_ID(N'dbo.DailyState', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DailyState (
        id INT IDENTITY(1,1) NOT NULL,
        user_id INT NOT NULL,
        date DATE NOT NULL,
        sleep_duration DECIMAL(4,2) NULL,
        sleep_quality INT NULL,
        energy INT NOT NULL,
        mood INT NOT NULL,
        stress INT NOT NULL,
        physical_state INT NOT NULL,
        has_illness BIT NOT NULL CONSTRAINT DF_DailyState_has_illness DEFAULT 0,
        has_pain_or_injury BIT NOT NULL CONSTRAINT DF_DailyState_has_pain_or_injury DEFAULT 0,
        calories_intake INT NULL,
        had_meals BIT NULL,
        meals_count INT NULL,
        overeating BIT NULL,
        undereating BIT NULL,
        activity NVARCHAR(100) NULL,
        activity_duration DECIMAL(5,2) NULL,
        rest_taken BIT NULL,
        screen_time DECIMAL(5,2) NULL,
        screen_before_sleep BIT NULL,
        day_type NVARCHAR(30) NULL,
        notes NVARCHAR(1000) NULL,
        activity_type NVARCHAR(50) NULL,
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_DailyState_updated_at DEFAULT SYSDATETIME(),
        CONSTRAINT PK_DailyState PRIMARY KEY (id),
        CONSTRAINT FK_DailyState_UserProfile FOREIGN KEY (user_id) REFERENCES dbo.UserProfile(id) ON DELETE CASCADE,
        CONSTRAINT UQ_DailyState_user_date UNIQUE (user_id, date),
        CONSTRAINT CK_DailyState_sleep_duration CHECK (sleep_duration IS NULL OR sleep_duration >= 0),
        CONSTRAINT CK_DailyState_sleep_quality CHECK (sleep_quality IS NULL OR sleep_quality BETWEEN 1 AND 5),
        CONSTRAINT CK_DailyState_energy CHECK (energy BETWEEN 1 AND 5),
        CONSTRAINT CK_DailyState_mood CHECK (mood BETWEEN 1 AND 5),
        CONSTRAINT CK_DailyState_stress CHECK (stress BETWEEN 1 AND 5),
        CONSTRAINT CK_DailyState_physical_state CHECK (physical_state BETWEEN 1 AND 5),
        CONSTRAINT CK_DailyState_calories_intake CHECK (calories_intake IS NULL OR calories_intake >= 0),
        CONSTRAINT CK_DailyState_meals_count CHECK (meals_count IS NULL OR meals_count >= 0),
        CONSTRAINT CK_DailyState_activity_duration CHECK (activity_duration IS NULL OR activity_duration >= 0),
        CONSTRAINT CK_DailyState_screen_time CHECK (screen_time IS NULL OR screen_time >= 0),
        CONSTRAINT CK_DailyState_activity_type CHECK (activity_type IS NULL OR activity_type IN ('walking', 'running', 'gym', 'cycling', 'stretching', 'yoga', 'other'))
    );
END;
""");

        migrationBuilder.Sql("""
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Habit_user_id' AND object_id = OBJECT_ID(N'dbo.Habit'))
    CREATE INDEX IX_Habit_user_id ON dbo.Habit(user_id);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Task_user_id' AND object_id = OBJECT_ID(N'dbo.Task'))
    CREATE INDEX IX_Task_user_id ON dbo.[Task](user_id);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Task_status' AND object_id = OBJECT_ID(N'dbo.Task'))
    CREATE INDEX IX_Task_status ON dbo.[Task](status);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DailyState_user_id_date' AND object_id = OBJECT_ID(N'dbo.DailyState'))
    CREATE INDEX IX_DailyState_user_id_date ON dbo.DailyState(user_id, date);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_HabitLog_date' AND object_id = OBJECT_ID(N'dbo.HabitLog'))
    CREATE INDEX IX_HabitLog_date ON dbo.HabitLog(date);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TaskHistory_task_id' AND object_id = OBJECT_ID(N'dbo.TaskHistory'))
    CREATE INDEX IX_TaskHistory_task_id ON dbo.TaskHistory(task_id);
""");

        migrationBuilder.Sql("""
IF OBJECT_ID(N'dbo.vw_ProductivityOverview', N'V') IS NULL
BEGIN
    EXEC(N'
CREATE VIEW dbo.vw_ProductivityOverview AS
SELECT
    ds.user_id,
    ds.date,
    ds.energy,
    ds.mood,
    ds.stress,
    ds.physical_state,
    (
        SELECT COUNT(*)
        FROM [Task] t
        WHERE t.user_id = ds.user_id
        AND t.planned_date = ds.date
    ) AS tasks_count,
    (
        SELECT COUNT(*)
        FROM [Task] t
        WHERE t.user_id = ds.user_id
        AND t.planned_date = ds.date
        AND t.status = ''completed''
    ) AS completed_tasks,
    (
        SELECT COUNT(*)
        FROM HabitLog hl
        INNER JOIN Habit h
            ON hl.habit_id = h.id
        WHERE h.user_id = ds.user_id
        AND hl.date = ds.date
    ) AS habit_logs,
    (
        SELECT COUNT(*)
        FROM HabitLog hl
        INNER JOIN Habit h
            ON hl.habit_id = h.id
        WHERE h.user_id = ds.user_id
        AND hl.date = ds.date
        AND hl.status = ''completed''
    ) AS completed_habits
FROM DailyState ds;');
END;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("IF OBJECT_ID(N'dbo.vw_ProductivityOverview', N'V') IS NOT NULL DROP VIEW dbo.vw_ProductivityOverview;");
        migrationBuilder.Sql("IF OBJECT_ID(N'dbo.DailyState', N'U') IS NOT NULL DROP TABLE dbo.DailyState;");
        migrationBuilder.Sql("IF OBJECT_ID(N'dbo.TaskHistory', N'U') IS NOT NULL DROP TABLE dbo.TaskHistory;");
        migrationBuilder.Sql("IF OBJECT_ID(N'dbo.HabitLog', N'U') IS NOT NULL DROP TABLE dbo.HabitLog;");
        migrationBuilder.Sql("IF OBJECT_ID(N'dbo.Task', N'U') IS NOT NULL DROP TABLE dbo.[Task];");
        migrationBuilder.Sql("IF OBJECT_ID(N'dbo.Habit', N'U') IS NOT NULL DROP TABLE dbo.Habit;");
        migrationBuilder.Sql("IF OBJECT_ID(N'dbo.UserProfile', N'U') IS NOT NULL DROP TABLE dbo.UserProfile;");
    }
}
