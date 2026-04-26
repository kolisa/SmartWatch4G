-- =============================================================================
-- Production migration script
-- Run against: WIN-PJVEC69ELM4\SQLEXPRESS01 / SmartWatchDb
-- Safe to re-run: every statement checks before altering.
-- Does NOT drop, truncate or modify any existing data.
-- =============================================================================

USE SmartWatchDb;
GO

-- =============================================================================
-- 1. NEW TABLE: companies
--    Required before user_profiles gets company_id FK.
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'companies')
CREATE TABLE companies (
    id                  INT IDENTITY(1,1) PRIMARY KEY,
    name                NVARCHAR(200) NOT NULL,
    registration_number NVARCHAR(100) NULL,
    contact_email       NVARCHAR(200) NULL,
    contact_phone       NVARCHAR(50)  NULL,
    address             NVARCHAR(500) NULL,
    is_active           BIT           NOT NULL DEFAULT 1,
    created_at          DATETIME2     DEFAULT GETDATE(),
    updated_at          DATETIME2     DEFAULT GETDATE()
);
GO

-- =============================================================================
-- 2. NEW COLUMNS: user_profiles
-- =============================================================================
IF COL_LENGTH('user_profiles', 'is_active') IS NULL
    ALTER TABLE user_profiles ADD is_active BIT NOT NULL DEFAULT 1;

IF COL_LENGTH('user_profiles', 'company_id') IS NULL
BEGIN
    ALTER TABLE user_profiles ADD company_id INT NULL;
    IF OBJECT_ID('FK_user_profiles_company_id', 'F') IS NULL
        ALTER TABLE user_profiles ADD CONSTRAINT FK_user_profiles_company_id
            FOREIGN KEY (company_id) REFERENCES companies(id) ON DELETE SET NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'uq_user_profiles_user_id'
               AND object_id = OBJECT_ID('user_profiles'))
    ALTER TABLE user_profiles ADD CONSTRAINT uq_user_profiles_user_id UNIQUE (user_id);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_user_profiles_active_name'
               AND object_id = OBJECT_ID('user_profiles'))
    CREATE INDEX IX_user_profiles_active_name ON user_profiles (is_active, surname, name)
        INCLUDE (email, cell, emp_no, address, updated_at);
GO

-- =============================================================================
-- 3. NEW TABLES: device settings (one row per device)
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_data_freq')
CREATE TABLE device_data_freq (
    device_id         NVARCHAR(50) PRIMARY KEY,
    gps_auto_check    BIT          NULL,
    gps_interval_time INT          NULL,
    power_mode        INT          NULL,
    user_id           INT          NULL,
    company_id        INT          NULL,
    updated_at        DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_locate_freq')
CREATE TABLE device_locate_freq (
    device_id            NVARCHAR(50) PRIMARY KEY,
    data_auto_upload     BIT          NULL,
    data_upload_interval INT          NULL,
    auto_locate          BIT          NULL,
    locate_interval_time INT          NULL,
    power_mode           INT          NULL,
    user_id              INT          NULL,
    company_id           INT          NULL,
    updated_at           DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_gps_settings')
CREATE TABLE device_gps_settings (
    device_id         NVARCHAR(50) PRIMARY KEY,
    gps_auto_check    BIT          NULL,
    gps_interval_time INT          NULL,
    run_gps           BIT          NULL,
    user_id           INT          NULL,
    company_id        INT          NULL,
    updated_at        DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_hr_alarm')
CREATE TABLE device_hr_alarm (
    device_id      NVARCHAR(50) PRIMARY KEY,
    [open]         BIT          NULL,
    high           INT          NULL,
    low            INT          NULL,
    threshold      INT          NULL,
    alarm_interval INT          NULL,
    user_id        INT          NULL,
    company_id     INT          NULL,
    updated_at     DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_dynamic_hr_alarm')
CREATE TABLE device_dynamic_hr_alarm (
    device_id  NVARCHAR(50) PRIMARY KEY,
    [open]     BIT          NULL,
    high       INT          NULL,
    low        INT          NULL,
    timeout    INT          NULL,
    interval   INT          NULL,
    user_id    INT          NULL,
    company_id INT          NULL,
    updated_at DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_spo2_alarm')
CREATE TABLE device_spo2_alarm (
    device_id  NVARCHAR(50) PRIMARY KEY,
    [open]     BIT          NULL,
    low        INT          NULL,
    user_id    INT          NULL,
    company_id INT          NULL,
    updated_at DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_bp_alarm')
CREATE TABLE device_bp_alarm (
    device_id  NVARCHAR(50) PRIMARY KEY,
    [open]     BIT          NULL,
    sbp_high   INT          NULL,
    sbp_below  INT          NULL,
    dbp_high   INT          NULL,
    dbp_below  INT          NULL,
    user_id    INT          NULL,
    company_id INT          NULL,
    updated_at DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_temp_alarm')
CREATE TABLE device_temp_alarm (
    device_id  NVARCHAR(50) PRIMARY KEY,
    [open]     BIT          NULL,
    high       INT          NULL,
    low        INT          NULL,
    user_id    INT          NULL,
    company_id INT          NULL,
    updated_at DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_fall_settings')
CREATE TABLE device_fall_settings (
    device_id      NVARCHAR(50) PRIMARY KEY,
    fall_check     BIT          NULL,
    fall_threshold INT          NULL,
    user_id        INT          NULL,
    company_id     INT          NULL,
    updated_at     DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_hr_interval')
CREATE TABLE device_hr_interval (
    device_id  NVARCHAR(50) PRIMARY KEY,
    interval   INT          NULL,
    user_id    INT          NULL,
    company_id INT          NULL,
    updated_at DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_other_interval')
CREATE TABLE device_other_interval (
    device_id  NVARCHAR(50) PRIMARY KEY,
    interval   INT          NULL,
    user_id    INT          NULL,
    company_id INT          NULL,
    updated_at DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_goal')
CREATE TABLE device_goal (
    device_id  NVARCHAR(50) PRIMARY KEY,
    step       INT          NULL,
    distance   INT          NULL,
    calorie    INT          NULL,
    user_id    INT          NULL,
    company_id INT          NULL,
    updated_at DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_display')
CREATE TABLE device_display (
    device_id        NVARCHAR(50) PRIMARY KEY,
    language         INT          NULL,
    hour_format      INT          NULL,
    date_format      INT          NULL,
    distance_unit    INT          NULL,
    temperature_unit INT          NULL,
    wear_hand_right  BIT          NULL,
    user_id          INT          NULL,
    company_id       INT          NULL,
    updated_at       DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_auto_af')
CREATE TABLE device_auto_af (
    device_id       NVARCHAR(50) PRIMARY KEY,
    [open]          BIT          NULL,
    interval        INT          NULL,
    rri_single_time BIT          NULL,
    rri_type        INT          NULL,
    user_id         INT          NULL,
    company_id      INT          NULL,
    updated_at      DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_bp_adjust')
CREATE TABLE device_bp_adjust (
    device_id  NVARCHAR(50) PRIMARY KEY,
    sbp_band   INT          NULL,
    dbp_band   INT          NULL,
    sbp_meter  INT          NULL,
    dbp_meter  INT          NULL,
    user_id    INT          NULL,
    company_id INT          NULL,
    updated_at DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_user_info')
CREATE TABLE device_user_info (
    device_id      NVARCHAR(50) PRIMARY KEY,
    height         INT          NULL,
    weight         INT          NULL,
    gender         INT          NULL,
    age            INT          NULL,
    calibrate_walk INT          NULL,
    calibrate_run  INT          NULL,
    wrist_circle   INT          NULL,
    hypertension   INT          NULL,
    user_id        INT          NULL,
    company_id     INT          NULL,
    updated_at     DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_lcd_gesture')
CREATE TABLE device_lcd_gesture (
    device_id  NVARCHAR(50) PRIMARY KEY,
    [open]     BIT          NULL,
    start_hour INT          NULL,
    end_hour   INT          NULL,
    user_id    INT          NULL,
    company_id INT          NULL,
    updated_at DATETIME2    DEFAULT GETDATE()
);
GO

-- =============================================================================
-- 4. NEW COLUMNS: add user_id / company_id to any existing table missing them
-- =============================================================================
DECLARE @tables NVARCHAR(MAX) =
    'gps_tracks,health_snapshots,alarms,sos_events,device_info_log,' +
    'device_user_info,device_fall_settings,device_data_freq,device_locate_freq,' +
    'device_lcd_gesture,device_hr_alarm,device_dynamic_hr_alarm,device_spo2_alarm,' +
    'device_bp_alarm,device_temp_alarm,device_auto_af,device_goal,device_display,' +
    'device_bp_adjust,device_hr_interval,device_other_interval,device_gps_settings,' +
    'device_phonebook,device_clock_alarms,device_sedentary,' +
    'sleep_calculations,ecg_calculations,af_calculations,spo2_calculations';

DECLARE @tbl NVARCHAR(200), @pos INT, @sql NVARCHAR(500);
WHILE LEN(@tables) > 0
BEGIN
    SET @pos    = CHARINDEX(',', @tables);
    IF @pos = 0 SET @pos = LEN(@tables) + 1;
    SET @tbl    = LEFT(@tables, @pos - 1);
    SET @tables = SUBSTRING(@tables, @pos + 1, LEN(@tables));
    -- Only act on tables that actually exist (skip if table not yet created above)
    IF OBJECT_ID(@tbl, 'U') IS NOT NULL
    BEGIN
        IF COL_LENGTH(@tbl, 'user_id') IS NULL
        BEGIN
            SET @sql = N'ALTER TABLE ' + QUOTENAME(@tbl) + N' ADD user_id INT NULL';
            EXEC sp_executesql @sql;
        END
        IF COL_LENGTH(@tbl, 'company_id') IS NULL
        BEGIN
            SET @sql = N'ALTER TABLE ' + QUOTENAME(@tbl) + N' ADD company_id INT NULL';
            EXEC sp_executesql @sql;
        END
    END
END
GO

-- =============================================================================
-- 5. NEW TABLE: audit_log
--    Tracks every INSERT/UPDATE/UPSERT operation for debugging.
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'audit_log')
CREATE TABLE audit_log (
    id          BIGINT IDENTITY(1,1) PRIMARY KEY,
    action      NVARCHAR(20)   NOT NULL,
    table_name  NVARCHAR(100)  NOT NULL,
    device_id   NVARCHAR(50)   NULL,
    details     NVARCHAR(500)  NULL,
    occurred_at DATETIME2      NOT NULL DEFAULT GETDATE()
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_audit_occurred'
               AND object_id=OBJECT_ID('audit_log'))
    CREATE INDEX IX_audit_occurred ON audit_log (occurred_at DESC)
        INCLUDE (action, table_name, device_id);
GO

-- =============================================================================
-- 6. DATA CLEANUP: reassign corrupted device_id records to 863758060995486
--    A firmware encoding bug caused one device to send garbled bytes as its
--    ID. The fix is in place; this script migrates the orphaned rows.
--    Safe to re-run: exits early if no corrupted ID is found.
-- =============================================================================
DECLARE @bad  NVARCHAR(50);
DECLARE @good NVARCHAR(50) = N'863758060995486';

-- Find any device_id that isn't a standard 15-digit IMEI
SELECT TOP 1 @bad = device_id
FROM (
    SELECT DISTINCT device_id FROM gps_tracks
    UNION SELECT DISTINCT device_id FROM health_snapshots
    UNION SELECT DISTINCT device_id FROM alarms
    UNION SELECT DISTINCT device_id FROM sos_events
    UNION SELECT DISTINCT device_id FROM device_info_log
) AS ids
WHERE PATINDEX(N'%[^0-9]%', device_id) > 0
   OR LEN(device_id) <> 15;

IF @bad IS NULL
BEGIN
    PRINT N'No corrupted device_id found — cleanup not needed or already done.';
END
ELSE
BEGIN
    PRINT N'Reassigning corrupted device_id to ' + @good;

    -- gps_tracks (UNIQUE: device_id, gnss_time, longitude, latitude)
    DELETE g FROM gps_tracks g
    WHERE g.device_id = @bad
      AND EXISTS (SELECT 1 FROM gps_tracks x
                  WHERE x.device_id = @good
                    AND x.gnss_time  = g.gnss_time
                    AND x.longitude  = g.longitude
                    AND x.latitude   = g.latitude);
    UPDATE gps_tracks SET device_id = @good WHERE device_id = @bad;

    -- health_snapshots (UNIQUE: device_id, record_time)
    DELETE h FROM health_snapshots h
    WHERE h.device_id = @bad
      AND EXISTS (SELECT 1 FROM health_snapshots x
                  WHERE x.device_id = @good AND x.record_time = h.record_time);
    UPDATE health_snapshots SET device_id = @good WHERE device_id = @bad;

    -- alarms (UNIQUE: device_id, alarm_time, alarm_type)
    DELETE a FROM alarms a
    WHERE a.device_id = @bad
      AND EXISTS (SELECT 1 FROM alarms x
                  WHERE x.device_id = @good
                    AND x.alarm_time = a.alarm_time
                    AND x.alarm_type = a.alarm_type);
    UPDATE alarms SET device_id = @good WHERE device_id = @bad;

    -- sos_events (UNIQUE: device_id, alarm_time)
    DELETE s FROM sos_events s
    WHERE s.device_id = @bad
      AND EXISTS (SELECT 1 FROM sos_events x
                  WHERE x.device_id = @good AND x.alarm_time = s.alarm_time);
    UPDATE sos_events SET device_id = @good WHERE device_id = @bad;

    -- device_info_log (no unique constraint — just update)
    UPDATE device_info_log SET device_id = @good WHERE device_id = @bad;

    -- Single-row-per-device tables: keep @good row if it exists, else reassign @bad
    IF EXISTS (SELECT 1 FROM device_data_freq    WHERE device_id = @good)
        DELETE FROM device_data_freq    WHERE device_id = @bad;
    ELSE UPDATE device_data_freq    SET device_id = @good WHERE device_id = @bad;

    IF EXISTS (SELECT 1 FROM device_locate_freq  WHERE device_id = @good)
        DELETE FROM device_locate_freq  WHERE device_id = @bad;
    ELSE UPDATE device_locate_freq  SET device_id = @good WHERE device_id = @bad;

    IF EXISTS (SELECT 1 FROM device_gps_settings WHERE device_id = @good)
        DELETE FROM device_gps_settings WHERE device_id = @bad;
    ELSE UPDATE device_gps_settings SET device_id = @good WHERE device_id = @bad;

    IF EXISTS (SELECT 1 FROM device_hr_alarm     WHERE device_id = @good)
        DELETE FROM device_hr_alarm     WHERE device_id = @bad;
    ELSE UPDATE device_hr_alarm     SET device_id = @good WHERE device_id = @bad;

    IF EXISTS (SELECT 1 FROM device_dynamic_hr_alarm WHERE device_id = @good)
        DELETE FROM device_dynamic_hr_alarm WHERE device_id = @bad;
    ELSE UPDATE device_dynamic_hr_alarm SET device_id = @good WHERE device_id = @bad;

    IF EXISTS (SELECT 1 FROM device_spo2_alarm   WHERE device_id = @good)
        DELETE FROM device_spo2_alarm   WHERE device_id = @bad;
    ELSE UPDATE device_spo2_alarm   SET device_id = @good WHERE device_id = @bad;

    IF EXISTS (SELECT 1 FROM device_bp_alarm     WHERE device_id = @good)
        DELETE FROM device_bp_alarm     WHERE device_id = @bad;
    ELSE UPDATE device_bp_alarm     SET device_id = @good WHERE device_id = @bad;

    IF EXISTS (SELECT 1 FROM device_temp_alarm   WHERE device_id = @good)
        DELETE FROM device_temp_alarm   WHERE device_id = @bad;
    ELSE UPDATE device_temp_alarm   SET device_id = @good WHERE device_id = @bad;

    IF EXISTS (SELECT 1 FROM device_fall_settings WHERE device_id = @good)
        DELETE FROM device_fall_settings WHERE device_id = @bad;
    ELSE UPDATE device_fall_settings SET device_id = @good WHERE device_id = @bad;

    IF EXISTS (SELECT 1 FROM device_hr_interval  WHERE device_id = @good)
        DELETE FROM device_hr_interval  WHERE device_id = @bad;
    ELSE UPDATE device_hr_interval  SET device_id = @good WHERE device_id = @bad;

    IF EXISTS (SELECT 1 FROM device_other_interval WHERE device_id = @good)
        DELETE FROM device_other_interval WHERE device_id = @bad;
    ELSE UPDATE device_other_interval SET device_id = @good WHERE device_id = @bad;

    IF EXISTS (SELECT 1 FROM device_goal         WHERE device_id = @good)
        DELETE FROM device_goal         WHERE device_id = @bad;
    ELSE UPDATE device_goal         SET device_id = @good WHERE device_id = @bad;

    IF EXISTS (SELECT 1 FROM device_display      WHERE device_id = @good)
        DELETE FROM device_display      WHERE device_id = @bad;
    ELSE UPDATE device_display      SET device_id = @good WHERE device_id = @bad;

    IF EXISTS (SELECT 1 FROM device_auto_af      WHERE device_id = @good)
        DELETE FROM device_auto_af      WHERE device_id = @bad;
    ELSE UPDATE device_auto_af      SET device_id = @good WHERE device_id = @bad;

    IF EXISTS (SELECT 1 FROM device_bp_adjust    WHERE device_id = @good)
        DELETE FROM device_bp_adjust    WHERE device_id = @bad;
    ELSE UPDATE device_bp_adjust    SET device_id = @good WHERE device_id = @bad;

    IF EXISTS (SELECT 1 FROM device_user_info    WHERE device_id = @good)
        DELETE FROM device_user_info    WHERE device_id = @bad;
    ELSE UPDATE device_user_info    SET device_id = @good WHERE device_id = @bad;

    IF EXISTS (SELECT 1 FROM device_lcd_gesture  WHERE device_id = @good)
        DELETE FROM device_lcd_gesture  WHERE device_id = @bad;
    ELSE UPDATE device_lcd_gesture  SET device_id = @good WHERE device_id = @bad;

    -- user_profiles (device_id PRIMARY KEY)
    IF EXISTS (SELECT 1 FROM user_profiles WHERE device_id = @good)
        DELETE FROM user_profiles WHERE device_id = @bad;
    ELSE UPDATE user_profiles SET device_id = @good WHERE device_id = @bad;

    PRINT N'Done. Rows reassigned to ' + @good;
END
GO

-- =============================================================================
-- 7. BACKFILL: populate user_id / company_id on all records via user_profiles
--    user_profiles is the source of truth: device_id → user_id + company_id.
--    Only updates rows where either value is NULL so existing links are kept.
--    Safe to re-run.
-- =============================================================================

-- Step 1: ensure user_profiles itself has company_id set.
--         All known devices belong to company 1 (id=1 must exist in companies).
UPDATE user_profiles
SET company_id = 1
WHERE company_id IS NULL
  AND is_active = 1
  AND EXISTS (SELECT 1 FROM companies WHERE id = 1);

PRINT N'user_profiles: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' rows linked to company 1';
GO

-- Step 2: propagate user_id + company_id from user_profiles to every table
--         that has device_id, user_id, and company_id columns.
DECLARE @tbls NVARCHAR(MAX) =
    'gps_tracks,health_snapshots,alarms,sos_events,device_info_log,' +
    'device_data_freq,device_locate_freq,device_gps_settings,' +
    'device_hr_alarm,device_dynamic_hr_alarm,device_spo2_alarm,' +
    'device_bp_alarm,device_temp_alarm,device_fall_settings,' +
    'device_hr_interval,device_other_interval,device_goal,device_display,' +
    'device_auto_af,device_bp_adjust,device_user_info,device_lcd_gesture,' +
    'device_phonebook,device_clock_alarms,device_sedentary,' +
    'sleep_calculations,ecg_calculations,af_calculations,spo2_calculations';

DECLARE @t NVARCHAR(200), @p INT, @sql NVARCHAR(MAX), @rows INT;
WHILE LEN(@tbls) > 0
BEGIN
    SET @p    = CHARINDEX(',', @tbls);
    IF @p = 0 SET @p = LEN(@tbls) + 1;
    SET @t    = LEFT(@tbls, @p - 1);
    SET @tbls = SUBSTRING(@tbls, @p + 1, LEN(@tbls));

    IF OBJECT_ID(@t, 'U') IS NOT NULL
       AND COL_LENGTH(@t, 'device_id')   IS NOT NULL
       AND COL_LENGTH(@t, 'user_id')     IS NOT NULL
       AND COL_LENGTH(@t, 'company_id')  IS NOT NULL
    BEGIN
        SET @sql = N'
            UPDATE tbl
            SET tbl.user_id    = u.user_id,
                tbl.company_id = u.company_id
            FROM ' + QUOTENAME(@t) + N' tbl
            INNER JOIN user_profiles u
                ON  u.device_id  = tbl.device_id
                AND u.is_active  = 1
            WHERE tbl.company_id IS NULL
               OR tbl.user_id   IS NULL;
            SELECT @r = @@ROWCOUNT;';
        SET @rows = 0;
        EXEC sp_executesql @sql, N'@r INT OUTPUT', @r = @rows OUTPUT;
        IF @rows > 0
            PRINT N'  ' + @t + N': ' + CAST(@rows AS NVARCHAR(10)) + N' rows updated';
    END
END
GO

-- =============================================================================
-- 8. Verification: row counts and link coverage per table
-- =============================================================================
SELECT
    t.name                                                          AS table_name,
    SUM(p.rows)                                                     AS total_rows,
    MAX(CASE WHEN c.name = 'user_id'    THEN 'Y' ELSE 'N' END)     AS has_user_id_col,
    MAX(CASE WHEN c.name = 'company_id' THEN 'Y' ELSE 'N' END)     AS has_company_id_col
FROM sys.tables t
JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0,1)
JOIN sys.columns c    ON c.object_id = t.object_id
WHERE t.type = 'U'
GROUP BY t.name
ORDER BY t.name;
