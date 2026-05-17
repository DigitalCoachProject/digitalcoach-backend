CREATE DATABASE DigitalCoachDb;
GO

USE DigitalCoachDb;
GO

-- =========================================================
-- USER PROFILE
-- =========================================================

CREATE TABLE UserProfile (
    id INT IDENTITY(1,1) NOT NULL,

    gender NVARCHAR(20) NULL,

    height DECIMAL(5,2) NOT NULL,
    weight DECIMAL(5,2) NOT NULL,

    birth_date DATE NOT NULL,

    email NVARCHAR(255) NOT NULL,
    password_hash NVARCHAR(500) NOT NULL,

    created_at DATETIME2 NOT NULL
        CONSTRAINT DF_UserProfile_created_at
        DEFAULT SYSUTCDATETIME(),

    updated_at DATETIME2 NOT NULL
        CONSTRAINT DF_UserProfile_updated_at
        DEFAULT SYSDATETIME(),

    CONSTRAINT PK_UserProfile
        PRIMARY KEY (id),

    CONSTRAINT UQ_UserProfile_email
        UNIQUE (email),

    CONSTRAINT CK_UserProfile_height
        CHECK (height > 0),

    CONSTRAINT CK_UserProfile_weight
        CHECK (weight > 0)
);
GO

-- =========================================================
-- HABIT
-- =========================================================

CREATE TABLE Habit (
    id INT IDENTITY(1,1) NOT NULL,

    user_id INT NOT NULL,

    name NVARCHAR(100) NOT NULL,

    description NVARCHAR(500) NULL,

    type NVARCHAR(20) NOT NULL,

    frequency INT NULL,

    days_of_week NVARCHAR(100) NULL,

    difficulty INT NOT NULL,

    start_date DATE NOT NULL,

    is_active BIT NOT NULL
        CONSTRAINT DF_Habit_is_active
        DEFAULT 1,

    updated_at DATETIME2 NOT NULL
        CONSTRAINT DF_Habit_updated_at
        DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Habit
        PRIMARY KEY (id),

    CONSTRAINT FK_Habit_UserProfile
        FOREIGN KEY (user_id)
        REFERENCES UserProfile(id)
        ON DELETE CASCADE,

    CONSTRAINT CK_Habit_type
        CHECK (type IN ('daily', 'weekly', 'specific_days')),

    CONSTRAINT CK_Habit_frequency
        CHECK (frequency IS NULL OR frequency BETWEEN 1 AND 7),

    CONSTRAINT CK_Habit_difficulty
        CHECK (difficulty BETWEEN 1 AND 5)
);
GO

-- =========================================================
-- HABIT LOG
-- =========================================================

CREATE TABLE HabitLog (
    id INT IDENTITY(1,1) NOT NULL,

    habit_id INT NOT NULL,

    date DATE NOT NULL,

    status NVARCHAR(20) NOT NULL,

    reason NVARCHAR(100) NULL,

    comment NVARCHAR(500) NULL,

    CONSTRAINT PK_HabitLog
        PRIMARY KEY (id),

    CONSTRAINT FK_HabitLog_Habit
        FOREIGN KEY (habit_id)
        REFERENCES Habit(id)
        ON DELETE CASCADE,

    CONSTRAINT UQ_HabitLog_habit_date
        UNIQUE (habit_id, date),

    CONSTRAINT CK_HabitLog_status
        CHECK (status IN ('completed', 'failed', 'skipped'))
);
GO

-- =========================================================
-- TASK
-- =========================================================

CREATE TABLE [Task] (
    id INT IDENTITY(1,1) NOT NULL,

    user_id INT NOT NULL,

    name NVARCHAR(100) NOT NULL,

    description NVARCHAR(500) NULL,

    created_at DATETIME2 NOT NULL
        CONSTRAINT DF_Task_created_at
        DEFAULT SYSUTCDATETIME(),

    planned_date DATE NOT NULL,

    deadline DATE NULL,

    priority INT NOT NULL,

    difficulty INT NOT NULL,

    status NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Task_status
        DEFAULT 'planned',

    completed_at DATETIME2 NULL,

    reschedule_count INT NOT NULL
        CONSTRAINT DF_Task_reschedule_count
        DEFAULT 0,

    updated_at DATETIME2 NOT NULL
        CONSTRAINT DF_Task_updated_at
        DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Task
        PRIMARY KEY (id),

    CONSTRAINT FK_Task_UserProfile
        FOREIGN KEY (user_id)
        REFERENCES UserProfile(id)
        ON DELETE CASCADE,

    CONSTRAINT CK_Task_priority
        CHECK (priority BETWEEN 1 AND 5),

    CONSTRAINT CK_Task_difficulty
        CHECK (difficulty BETWEEN 1 AND 5),

    CONSTRAINT CK_Task_status
        CHECK (status IN ('planned', 'completed', 'overdue', 'cancelled')),

    CONSTRAINT CK_Task_reschedule_count
        CHECK (reschedule_count >= 0)
);
GO

-- =========================================================
-- TASK HISTORY
-- =========================================================

CREATE TABLE TaskHistory (
    id INT IDENTITY(1,1) NOT NULL,

    task_id INT NOT NULL,

    change_date DATETIME2 NOT NULL
        CONSTRAINT DF_TaskHistory_change_date
        DEFAULT SYSUTCDATETIME(),

    old_date DATE NOT NULL,

    new_date DATE NOT NULL,

    reason NVARCHAR(100) NULL,

    CONSTRAINT PK_TaskHistory
        PRIMARY KEY (id),

    CONSTRAINT FK_TaskHistory_Task
        FOREIGN KEY (task_id)
        REFERENCES [Task](id)
        ON DELETE CASCADE
);
GO

-- =========================================================
-- DAILY STATE
-- =========================================================

CREATE TABLE DailyState (
    id INT IDENTITY(1,1) NOT NULL,

    user_id INT NOT NULL,

    date DATE NOT NULL,

    sleep_duration DECIMAL(4,2) NULL,

    sleep_quality INT NULL,

    energy INT NOT NULL,

    mood INT NOT NULL,

    stress INT NOT NULL,

    physical_state INT NOT NULL,

    has_illness BIT NOT NULL
        CONSTRAINT DF_DailyState_has_illness
        DEFAULT 0,

    has_pain_or_injury BIT NOT NULL
        CONSTRAINT DF_DailyState_has_pain_or_injury
        DEFAULT 0,

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

    updated_at DATETIME2 NOT NULL
        CONSTRAINT DF_DailyState_updated_at
        DEFAULT SYSDATETIME(),

    CONSTRAINT PK_DailyState
        PRIMARY KEY (id),

    CONSTRAINT FK_DailyState_UserProfile
        FOREIGN KEY (user_id)
        REFERENCES UserProfile(id)
        ON DELETE CASCADE,

    CONSTRAINT UQ_DailyState_user_date
        UNIQUE (user_id, date),

    CONSTRAINT CK_DailyState_sleep_duration
        CHECK (sleep_duration IS NULL OR sleep_duration >= 0),

    CONSTRAINT CK_DailyState_sleep_quality
        CHECK (sleep_quality IS NULL OR sleep_quality BETWEEN 1 AND 5),

    CONSTRAINT CK_DailyState_energy
        CHECK (energy BETWEEN 1 AND 5),

    CONSTRAINT CK_DailyState_mood
        CHECK (mood BETWEEN 1 AND 5),

    CONSTRAINT CK_DailyState_stress
        CHECK (stress BETWEEN 1 AND 5),

    CONSTRAINT CK_DailyState_physical_state
        CHECK (physical_state BETWEEN 1 AND 5),

    CONSTRAINT CK_DailyState_calories_intake
        CHECK (calories_intake IS NULL OR calories_intake >= 0),

    CONSTRAINT CK_DailyState_meals_count
        CHECK (meals_count IS NULL OR meals_count >= 0),

    CONSTRAINT CK_DailyState_activity_duration
        CHECK (activity_duration IS NULL OR activity_duration >= 0),

    CONSTRAINT CK_DailyState_screen_time
        CHECK (screen_time IS NULL OR screen_time >= 0),

    CONSTRAINT CK_DailyState_activity_type
        CHECK (
            activity_type IS NULL
            OR activity_type IN (
                'walking',
                'running',
                'gym',
                'cycling',
                'stretching',
                'yoga',
                'other'
            )
        )
);
GO

-- =========================================================
-- RECOMMENDATION
-- =========================================================

CREATE TABLE Recommendation (
    id INT IDENTITY(1,1) NOT NULL,

    user_id INT NOT NULL,

    type NVARCHAR(30) NOT NULL,

    title NVARCHAR(150) NOT NULL,

    message NVARCHAR(1000) NOT NULL,

    priority INT NOT NULL,

    is_read BIT NOT NULL
        CONSTRAINT DF_Recommendation_is_read
        DEFAULT 0,

    created_at DATETIME2 NOT NULL
        CONSTRAINT DF_Recommendation_created_at
        DEFAULT SYSUTCDATETIME(),

    expires_at DATETIME2 NULL,

    CONSTRAINT PK_Recommendation
        PRIMARY KEY (id),

    CONSTRAINT FK_Recommendation_UserProfile
        FOREIGN KEY (user_id)
        REFERENCES UserProfile(id)
        ON DELETE CASCADE,

    CONSTRAINT CK_Recommendation_priority
        CHECK (priority BETWEEN 1 AND 5),

    CONSTRAINT CK_Recommendation_type
        CHECK (type IN (
            'productivity',
            'wellness',
            'habit',
            'task',
            'burnout',
            'sleep',
            'motivation'
        ))
);
GO

-- =========================================================
-- NOTIFICATION
-- =========================================================

CREATE TABLE Notification (
    id INT IDENTITY(1,1) NOT NULL,

    user_id INT NOT NULL,

    type NVARCHAR(30) NOT NULL,

    title NVARCHAR(150) NOT NULL,

    message NVARCHAR(1000) NOT NULL,

    priority INT NOT NULL,

    is_read BIT NOT NULL
        CONSTRAINT DF_Notification_is_read
        DEFAULT 0,

    created_at DATETIME2 NOT NULL
        CONSTRAINT DF_Notification_created_at
        DEFAULT SYSUTCDATETIME(),

    scheduled_for DATETIME2 NULL,

    read_at DATETIME2 NULL,

    expires_at DATETIME2 NULL,

    CONSTRAINT PK_Notification
        PRIMARY KEY (id),

    CONSTRAINT FK_Notification_UserProfile
        FOREIGN KEY (user_id)
        REFERENCES UserProfile(id)
        ON DELETE CASCADE,

    CONSTRAINT CK_Notification_priority
        CHECK (priority BETWEEN 1 AND 5),

    CONSTRAINT CK_Notification_type
        CHECK (type IN (
            'habit',
            'task',
            'wellness',
            'recommendation',
            'burnout',
            'reminder',
            'system'
        ))
);
GO

-- =========================================================
-- INDEXES
-- =========================================================

CREATE INDEX IX_Habit_user_id
ON Habit(user_id);
GO

CREATE INDEX IX_Task_user_id
ON [Task](user_id);
GO

CREATE INDEX IX_Task_status
ON [Task](status);
GO

CREATE INDEX IX_DailyState_user_id_date
ON DailyState(user_id, date);
GO

CREATE INDEX IX_HabitLog_date
ON HabitLog(date);
GO

CREATE INDEX IX_TaskHistory_task_id
ON TaskHistory(task_id);
GO

CREATE INDEX IX_Recommendation_user_id
ON Recommendation(user_id);
GO

CREATE INDEX IX_Recommendation_user_type_created_at
ON Recommendation(user_id, type, created_at);
GO

CREATE INDEX IX_Notification_user_id
ON Notification(user_id);
GO

CREATE INDEX IX_Notification_user_is_read_created_at
ON Notification(user_id, is_read, created_at);
GO

CREATE INDEX IX_Notification_user_type_created_at
ON Notification(user_id, type, created_at);
GO

-- =========================================================
-- ANALYTICS VIEW
-- =========================================================

CREATE VIEW vw_ProductivityOverview AS
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
        AND t.status = 'completed'
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
        AND hl.status = 'completed'
    ) AS completed_habits

FROM DailyState ds;
GO
