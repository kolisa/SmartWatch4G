-- =============================================================================
-- SmartWatch4G  –  Schema & Index Migration Script
-- Run this on any existing SmartWatchDb instance.
-- All statements are idempotent (safe to run multiple times).
-- =============================================================================

USE SmartWatchDb;
GO

-- =============================================================================
-- 1.  NEW TABLE:  companies
--     Represents a mining company. A worker (user_profile) belongs to one company.
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'companies')
BEGIN
    CREATE TABLE companies (
        id                  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_companies PRIMARY KEY,
        name                NVARCHAR(200) NOT NULL,
        registration_number NVARCHAR(100) NULL,
        contact_email       NVARCHAR(200) NULL,
        contact_phone       NVARCHAR(50)  NULL,
        address             NVARCHAR(500) NULL,
        is_active           BIT           NOT NULL DEFAULT 1,
        created_at          DATETIME2     NOT NULL DEFAULT GETDATE(),
        updated_at          DATETIME2     NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Created table companies';
END
ELSE
    PRINT 'Table companies already exists — skipped';
GO

-- =============================================================================
-- 2.  NEW TABLE / ALTER TABLE:  user_profiles
--     Links a device to a mine worker's personal details.
--     New columns: user_id (surrogate key), company_id (FK to companies).
--     device_id remains the PRIMARY KEY (one worker per device).
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'user_profiles')
BEGIN
    CREATE TABLE user_profiles (
        device_id   NVARCHAR(50)  NOT NULL  CONSTRAINT PK_user_profiles PRIMARY KEY,
        user_id     INT           IDENTITY(1,1) NOT NULL,
        name        NVARCHAR(100) NOT NULL,
        surname     NVARCHAR(100) NOT NULL,
        email       NVARCHAR(200) NULL,
        cell        NVARCHAR(30)  NULL,
        emp_no      NVARCHAR(50)  NULL,
        address     NVARCHAR(500) NULL,
        company_id  INT           NULL  REFERENCES companies(id) ON DELETE SET NULL,
        is_active   BIT           NOT NULL  DEFAULT 1,
        updated_at  DATETIME2     NOT NULL  DEFAULT GETDATE(),
        CONSTRAINT uq_user_profiles_user_id UNIQUE (user_id)
    );
    PRINT 'Created table user_profiles';
END
ELSE
    PRINT 'Table user_profiles already exists — skipped';
GO

-- 2a. Add is_active (backfill for tables created before this column was added)
IF COL_LENGTH('user_profiles', 'is_active') IS NULL
BEGIN
    ALTER TABLE user_profiles ADD is_active BIT NOT NULL DEFAULT 1;
    PRINT 'Added column user_profiles.is_active';
END
ELSE
    PRINT 'Column user_profiles.is_active already exists — skipped';
GO

-- 2b. Add user_id surrogate key
IF COL_LENGTH('user_profiles', 'user_id') IS NULL
BEGIN
    ALTER TABLE user_profiles ADD user_id INT IDENTITY(1,1) NOT NULL;
    PRINT 'Added column user_profiles.user_id';
END
ELSE
    PRINT 'Column user_profiles.user_id already exists — skipped';
GO

-- 2c. Unique constraint on user_id
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name='uq_user_profiles_user_id' AND object_id=OBJECT_ID('user_profiles'))
BEGIN
    ALTER TABLE user_profiles ADD CONSTRAINT uq_user_profiles_user_id UNIQUE (user_id);
    PRINT 'Added unique constraint uq_user_profiles_user_id';
END
ELSE
    PRINT 'Constraint uq_user_profiles_user_id already exists — skipped';
GO

-- 2d. Add company_id FK (user can belong to exactly one company)
IF COL_LENGTH('user_profiles', 'company_id') IS NULL
BEGIN
    ALTER TABLE user_profiles ADD company_id INT NULL;
    IF OBJECT_ID('FK_user_profiles_company_id', 'F') IS NULL
        ALTER TABLE user_profiles
            ADD CONSTRAINT FK_user_profiles_company_id
            FOREIGN KEY (company_id) REFERENCES companies(id) ON DELETE SET NULL;
    PRINT 'Added column user_profiles.company_id with FK to companies';
END
ELSE
    PRINT 'Column user_profiles.company_id already exists — skipped';
GO

-- =============================================================================
-- 3.  DEDUP CONSTRAINTS on multi-row-per-device tables
--     Settings tables already use device_id as PRIMARY KEY (1 row per device).
--     These three tables can accumulate duplicate rows — unique constraints prevent that.
-- =============================================================================

-- device_phonebook: one entry per (device, phone number)
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name='UQ_phonebook_device_number' AND object_id=OBJECT_ID('device_phonebook'))
BEGIN
    ALTER TABLE device_phonebook
        ADD CONSTRAINT UQ_phonebook_device_number UNIQUE (device_id, number);
    PRINT 'Added unique constraint UQ_phonebook_device_number';
END
ELSE
    PRINT 'Constraint UQ_phonebook_device_number already exists — skipped';
GO

-- device_clock_alarms: one alarm per (device, hour, minute)
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name='UQ_clock_alarm_device_time' AND object_id=OBJECT_ID('device_clock_alarms'))
BEGIN
    ALTER TABLE device_clock_alarms
        ADD CONSTRAINT UQ_clock_alarm_device_time UNIQUE (device_id, hour, minute);
    PRINT 'Added unique constraint UQ_clock_alarm_device_time';
END
ELSE
    PRINT 'Constraint UQ_clock_alarm_device_time already exists — skipped';
GO

-- device_sedentary: one reminder per (device, start_hour, end_hour)
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name='UQ_sedentary_device_window' AND object_id=OBJECT_ID('device_sedentary'))
BEGIN
    ALTER TABLE device_sedentary
        ADD CONSTRAINT UQ_sedentary_device_window UNIQUE (device_id, start_hour, end_hour);
    PRINT 'Added unique constraint UQ_sedentary_device_window';
END
ELSE
    PRINT 'Constraint UQ_sedentary_device_window already exists — skipped';
GO

-- =============================================================================
-- 4.  INDEXES
--     Every index creation is guarded with IF NOT EXISTS so reruns are safe.
-- =============================================================================

-- ---------------------------------------------------------------------------
-- gps_tracks
--
--   IX_gps_device_id      → used by GetLatestGnssTrack
--                           SELECT TOP 1 … WHERE device_id = ? ORDER BY id DESC
--
--   IX_gps_device_created → used by GetGnssTracks
--                           WHERE device_id = ? AND created_at BETWEEN ? AND ?
--                           ORDER BY created_at ASC
-- ---------------------------------------------------------------------------

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_gps_device_id' AND object_id = OBJECT_ID('gps_tracks'))
BEGIN
    CREATE INDEX IX_gps_device_id
        ON gps_tracks (device_id, id DESC)
        INCLUDE (gnss_time, longitude, latitude, loc_type, created_at);
    PRINT 'Created index IX_gps_device_id';
END
ELSE
    PRINT 'Index IX_gps_device_id already exists — skipped';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_gps_device_created' AND object_id = OBJECT_ID('gps_tracks'))
BEGIN
    CREATE INDEX IX_gps_device_created
        ON gps_tracks (device_id, created_at ASC)
        INCLUDE (gnss_time, longitude, latitude, loc_type, id);
    PRINT 'Created index IX_gps_device_created';
END
ELSE
    PRINT 'Index IX_gps_device_created already exists — skipped';
GO

-- ---------------------------------------------------------------------------
-- health_snapshots
--
--   IX_health_device_id   → used by GetLatestHealthSnapshot
--                           SELECT TOP 1 … WHERE device_id = ? ORDER BY id DESC
-- ---------------------------------------------------------------------------

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_health_device_id' AND object_id = OBJECT_ID('health_snapshots'))
BEGIN
    CREATE INDEX IX_health_device_id
        ON health_snapshots (device_id, id DESC)
        INCLUDE (record_time, battery, rssi, steps, distance, calorie,
                 avg_hr, max_hr, min_hr, avg_spo2, sbp, dbp, fatigue, created_at);
    PRINT 'Created index IX_health_device_id';
END
ELSE
    PRINT 'Index IX_health_device_id already exists — skipped';
GO

-- ---------------------------------------------------------------------------
-- alarms
--
--   IX_alarms_created_at  → used by GetRecentAlarmCount and GetRecentAlarms
--                           WHERE created_at >= DATEADD(…)
--                           ORDER BY created_at DESC
--                           (also covers JOIN to user_profiles by INCLUDE device_id)
-- ---------------------------------------------------------------------------

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_alarms_created_at' AND object_id = OBJECT_ID('alarms'))
BEGIN
    CREATE INDEX IX_alarms_created_at
        ON alarms (created_at DESC)
        INCLUDE (device_id, alarm_time, alarm_type, details);
    PRINT 'Created index IX_alarms_created_at';
END
ELSE
    PRINT 'Index IX_alarms_created_at already exists — skipped';
GO

-- ---------------------------------------------------------------------------
-- sos_events
--
--   IX_sos_created_at     → used by GetRecentSosCount
--                           WHERE created_at >= DATEADD(…)
-- ---------------------------------------------------------------------------

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_sos_created_at' AND object_id = OBJECT_ID('sos_events'))
BEGIN
    CREATE INDEX IX_sos_created_at
        ON sos_events (created_at DESC);
    PRINT 'Created index IX_sos_created_at';
END
ELSE
    PRINT 'Index IX_sos_created_at already exists — skipped';
GO

-- ---------------------------------------------------------------------------
-- user_profiles
--
--   IX_user_profiles_active_name → used by:
--     • GetAllUserProfiles        WHERE is_active=1 ORDER BY surname, name
--     • GetPagedUserProfiles      WHERE is_active=1 ORDER BY surname, name OFFSET/FETCH
--     • GetActiveWorkerCount      WHERE is_active=1  (COUNT)
--     • GetDashboardCounts        WHERE is_active=1  (sub-query COUNT)
-- ---------------------------------------------------------------------------

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_user_profiles_active_name' AND object_id = OBJECT_ID('user_profiles'))
BEGIN
    CREATE INDEX IX_user_profiles_active_name
        ON user_profiles (is_active, surname, name)
        INCLUDE (email, cell, emp_no, address, updated_at);
    PRINT 'Created index IX_user_profiles_active_name';
END
ELSE
    PRINT 'Index IX_user_profiles_active_name already exists — skipped';
GO

-- =============================================================================
-- 5.  ADD user_id / company_id TO ALL TELEMETRY AND SETTINGS TABLES
--     Both columns are nullable — NULL means the device had no linked user profile
--     at the time of the write. Values are auto-populated from user_profiles.
--     All ALTER TABLE statements are idempotent.
-- =============================================================================

DECLARE @tbl2 NVARCHAR(200), @pos2 INT, @sql2 NVARCHAR(500);
DECLARE @tables2 NVARCHAR(MAX) =
    'gps_tracks,health_snapshots,alarms,sos_events,device_info_log,' +
    'device_user_info,device_fall_settings,device_data_freq,device_locate_freq,' +
    'device_lcd_gesture,device_hr_alarm,device_dynamic_hr_alarm,device_spo2_alarm,' +
    'device_bp_alarm,device_temp_alarm,device_auto_af,device_goal,device_display,' +
    'device_bp_adjust,device_hr_interval,device_other_interval,device_gps_settings,' +
    'device_phonebook,device_clock_alarms,device_sedentary,' +
    'sleep_calculations,ecg_calculations,af_calculations,spo2_calculations';

WHILE LEN(@tables2) > 0
BEGIN
    SET @pos2   = CHARINDEX(',', @tables2);
    IF @pos2    = 0 SET @pos2 = LEN(@tables2) + 1;
    SET @tbl2   = LEFT(@tables2, @pos2 - 1);
    SET @tables2 = SUBSTRING(@tables2, @pos2 + 1, LEN(@tables2));
    IF COL_LENGTH(@tbl2, 'user_id') IS NULL
    BEGIN
        SET @sql2 = N'ALTER TABLE ' + QUOTENAME(@tbl2) + N' ADD user_id INT NULL';
        EXEC sp_executesql @sql2;
        PRINT 'Added user_id to ' + @tbl2;
    END
    IF COL_LENGTH(@tbl2, 'company_id') IS NULL
    BEGIN
        SET @sql2 = N'ALTER TABLE ' + QUOTENAME(@tbl2) + N' ADD company_id INT NULL';
        EXEC sp_executesql @sql2;
        PRINT 'Added company_id to ' + @tbl2;
    END
END
GO

-- =============================================================================
-- Summary of all indexes after this script
-- =============================================================================

SELECT
    t.name          AS [table],
    i.name          AS [index],
    i.type_desc     AS [type],
    STRING_AGG(c.name, ', ') WITHIN GROUP (ORDER BY ic.key_ordinal) AS [key_columns]
FROM sys.indexes i
JOIN sys.tables  t  ON t.object_id = i.object_id
JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 0
JOIN sys.columns c  ON c.object_id = ic.object_id AND c.column_id = ic.column_id
WHERE t.name IN ('gps_tracks','health_snapshots','alarms','sos_events','user_profiles',
                 'companies','device_phonebook','device_clock_alarms','device_sedentary')
  AND i.name LIKE 'IX_%' OR i.name LIKE 'UQ_%' OR i.name LIKE 'uq_%'
GROUP BY t.name, i.name, i.type_desc
ORDER BY t.name, i.name;
GO
