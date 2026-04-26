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
-- 6. Verification: show every table + column count + new columns present
-- =============================================================================
SELECT
    t.name                                              AS table_name,
    COUNT(c.column_id)                                  AS column_count,
    MAX(CASE WHEN c.name = 'user_id'    THEN 'Y' END)  AS has_user_id,
    MAX(CASE WHEN c.name = 'company_id' THEN 'Y' END)  AS has_company_id
FROM sys.tables t
JOIN sys.columns c ON c.object_id = t.object_id
WHERE t.type = 'U'
GROUP BY t.name
ORDER BY t.name;
