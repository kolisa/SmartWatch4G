-- =============================================================================
-- SmartWatchDb full schema setup
-- Run this against SmartWatchDb on WIN-PJVEC69ELM4\SQLEXPRESS01
-- Safe to re-run: all CREATE TABLE / ALTER TABLE statements are idempotent.
-- =============================================================================

USE SmartWatchDb;
GO

-- ── companies ─────────────────────────────────────────────────────────────────
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

-- ── user_profiles ─────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'user_profiles')
BEGIN
    CREATE TABLE user_profiles (
        device_id   NVARCHAR(50)  PRIMARY KEY,
        user_id     INT           IDENTITY(1,1) NOT NULL,
        name        NVARCHAR(100) NOT NULL,
        surname     NVARCHAR(100) NOT NULL,
        email       NVARCHAR(200) NULL,
        cell        NVARCHAR(30)  NULL,
        emp_no      NVARCHAR(50)  NULL,
        address     NVARCHAR(500) NULL,
        company_id  INT           NULL REFERENCES companies(id) ON DELETE SET NULL,
        is_active   BIT           NOT NULL DEFAULT 1,
        updated_at  DATETIME2     DEFAULT GETDATE(),
        CONSTRAINT uq_user_profiles_user_id UNIQUE (user_id)
    );
END
ELSE
BEGIN
    IF COL_LENGTH('user_profiles', 'is_active')  IS NULL ALTER TABLE user_profiles ADD is_active BIT NOT NULL DEFAULT 1;
    IF COL_LENGTH('user_profiles', 'company_id') IS NULL
    BEGIN
        ALTER TABLE user_profiles ADD company_id INT NULL;
        IF OBJECT_ID('FK_user_profiles_company_id', 'F') IS NULL
            ALTER TABLE user_profiles ADD CONSTRAINT FK_user_profiles_company_id
                FOREIGN KEY (company_id) REFERENCES companies(id) ON DELETE SET NULL;
    END
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='uq_user_profiles_user_id' AND object_id=OBJECT_ID('user_profiles'))
        ALTER TABLE user_profiles ADD CONSTRAINT uq_user_profiles_user_id UNIQUE (user_id);
END
GO

-- ── gps_tracks ────────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'gps_tracks')
CREATE TABLE gps_tracks (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    device_id   NVARCHAR(50)  NOT NULL,
    gnss_time   NVARCHAR(30)  NOT NULL,
    longitude   FLOAT         NOT NULL,
    latitude    FLOAT         NOT NULL,
    loc_type    NVARCHAR(30)  NULL,
    user_id     INT           NULL,
    company_id  INT           NULL,
    created_at  DATETIME2     DEFAULT GETDATE(),
    CONSTRAINT uq_gps UNIQUE (device_id, gnss_time, longitude, latitude)
);
GO

-- ── health_snapshots ──────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'health_snapshots')
CREATE TABLE health_snapshots (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    device_id   NVARCHAR(50)  NOT NULL,
    record_time NVARCHAR(30)  NOT NULL,
    battery     INT           NULL,
    rssi        INT           NULL,
    steps       INT           NULL,
    distance    FLOAT         NULL,
    calorie     FLOAT         NULL,
    avg_hr      INT           NULL,
    max_hr      INT           NULL,
    min_hr      INT           NULL,
    avg_spo2    INT           NULL,
    sbp         INT           NULL,
    dbp         INT           NULL,
    fatigue     INT           NULL,
    user_id     INT           NULL,
    company_id  INT           NULL,
    created_at  DATETIME2     DEFAULT GETDATE(),
    CONSTRAINT uq_health UNIQUE (device_id, record_time)
);
GO

-- ── alarms ────────────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'alarms')
CREATE TABLE alarms (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    device_id   NVARCHAR(50)  NOT NULL,
    alarm_time  NVARCHAR(30)  NOT NULL,
    alarm_type  NVARCHAR(30)  NOT NULL,
    details     NVARCHAR(200) NULL,
    user_id     INT           NULL,
    company_id  INT           NULL,
    created_at  DATETIME2     DEFAULT GETDATE(),
    CONSTRAINT uq_alarm UNIQUE (device_id, alarm_time, alarm_type)
);
GO

-- ── sos_events ────────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'sos_events')
CREATE TABLE sos_events (
    id           INT IDENTITY(1,1) PRIMARY KEY,
    device_id    NVARCHAR(50)  NOT NULL,
    alarm_time   NVARCHAR(30)  NOT NULL,
    latitude     FLOAT         NULL,
    longitude    FLOAT         NULL,
    call_number  NVARCHAR(30)  NULL,
    call_status  INT           NULL,
    call_start   NVARCHAR(30)  NULL,
    call_end     NVARCHAR(30)  NULL,
    user_id      INT           NULL,
    company_id   INT           NULL,
    created_at   DATETIME2     DEFAULT GETDATE(),
    CONSTRAINT uq_sos UNIQUE (device_id, alarm_time)
);
GO

-- ── device_info_log ───────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_info_log')
CREATE TABLE device_info_log (
    id             INT IDENTITY(1,1) PRIMARY KEY,
    device_id      NVARCHAR(50)   NOT NULL,
    recorded_at    NVARCHAR(30)   NOT NULL,
    model          NVARCHAR(50)   NULL,
    version        NVARCHAR(50)   NULL,
    wearing_status NVARCHAR(10)   NULL,
    signal         NVARCHAR(50)   NULL,
    raw_json       NVARCHAR(MAX)  NULL,
    user_id        INT            NULL,
    company_id     INT            NULL,
    created_at     DATETIME2      DEFAULT GETDATE()
);
GO

-- ── device_data_freq ──────────────────────────────────────────────────────────
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
GO

-- ── device_locate_freq ────────────────────────────────────────────────────────
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
GO

-- ── device_gps_settings ───────────────────────────────────────────────────────
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
GO

-- ── device_hr_alarm ───────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_hr_alarm')
CREATE TABLE device_hr_alarm (
    device_id      NVARCHAR(50) PRIMARY KEY,
    open           BIT          NULL,
    high           INT          NULL,
    low            INT          NULL,
    threshold      INT          NULL,
    alarm_interval INT          NULL,
    user_id        INT          NULL,
    company_id     INT          NULL,
    updated_at     DATETIME2    DEFAULT GETDATE()
);
GO

-- ── device_dynamic_hr_alarm ───────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_dynamic_hr_alarm')
CREATE TABLE device_dynamic_hr_alarm (
    device_id  NVARCHAR(50) PRIMARY KEY,
    open       BIT          NULL,
    high       INT          NULL,
    low        INT          NULL,
    timeout    INT          NULL,
    interval   INT          NULL,
    user_id    INT          NULL,
    company_id INT          NULL,
    updated_at DATETIME2    DEFAULT GETDATE()
);
GO

-- ── device_spo2_alarm ─────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_spo2_alarm')
CREATE TABLE device_spo2_alarm (
    device_id  NVARCHAR(50) PRIMARY KEY,
    open       BIT          NULL,
    low        INT          NULL,
    user_id    INT          NULL,
    company_id INT          NULL,
    updated_at DATETIME2    DEFAULT GETDATE()
);
GO

-- ── device_bp_alarm ───────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_bp_alarm')
CREATE TABLE device_bp_alarm (
    device_id  NVARCHAR(50) PRIMARY KEY,
    open       BIT          NULL,
    sbp_high   INT          NULL,
    sbp_below  INT          NULL,
    dbp_high   INT          NULL,
    dbp_below  INT          NULL,
    user_id    INT          NULL,
    company_id INT          NULL,
    updated_at DATETIME2    DEFAULT GETDATE()
);
GO

-- ── device_temp_alarm ─────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_temp_alarm')
CREATE TABLE device_temp_alarm (
    device_id  NVARCHAR(50) PRIMARY KEY,
    open       BIT          NULL,
    high       INT          NULL,
    low        INT          NULL,
    user_id    INT          NULL,
    company_id INT          NULL,
    updated_at DATETIME2    DEFAULT GETDATE()
);
GO

-- ── device_fall_settings ──────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_fall_settings')
CREATE TABLE device_fall_settings (
    device_id      NVARCHAR(50) PRIMARY KEY,
    fall_check     BIT          NULL,
    fall_threshold INT          NULL,
    user_id        INT          NULL,
    company_id     INT          NULL,
    updated_at     DATETIME2    DEFAULT GETDATE()
);
GO

-- ── device_hr_interval ────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_hr_interval')
CREATE TABLE device_hr_interval (
    device_id  NVARCHAR(50) PRIMARY KEY,
    interval   INT          NULL,
    user_id    INT          NULL,
    company_id INT          NULL,
    updated_at DATETIME2    DEFAULT GETDATE()
);
GO

-- ── device_other_interval ─────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_other_interval')
CREATE TABLE device_other_interval (
    device_id  NVARCHAR(50) PRIMARY KEY,
    interval   INT          NULL,
    user_id    INT          NULL,
    company_id INT          NULL,
    updated_at DATETIME2    DEFAULT GETDATE()
);
GO

-- ── device_goal ───────────────────────────────────────────────────────────────
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
GO

-- ── device_display ────────────────────────────────────────────────────────────
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
GO

-- ── device_auto_af ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_auto_af')
CREATE TABLE device_auto_af (
    device_id       NVARCHAR(50) PRIMARY KEY,
    open            BIT          NULL,
    interval        INT          NULL,
    rri_single_time BIT          NULL,
    rri_type        INT          NULL,
    user_id         INT          NULL,
    company_id      INT          NULL,
    updated_at      DATETIME2    DEFAULT GETDATE()
);
GO

-- ── device_bp_adjust ──────────────────────────────────────────────────────────
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
GO

-- ── device_user_info ──────────────────────────────────────────────────────────
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
GO

-- ── device_lcd_gesture ────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_lcd_gesture')
CREATE TABLE device_lcd_gesture (
    device_id  NVARCHAR(50) PRIMARY KEY,
    open       BIT          NULL,
    start_hour INT          NULL,
    end_hour   INT          NULL,
    user_id    INT          NULL,
    company_id INT          NULL,
    updated_at DATETIME2    DEFAULT GETDATE()
);
GO

-- ── device_phonebook ──────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_phonebook')
CREATE TABLE device_phonebook (
    id         INT IDENTITY(1,1) PRIMARY KEY,
    device_id  NVARCHAR(50)  NOT NULL,
    name       NVARCHAR(100) NOT NULL,
    number     NVARCHAR(30)  NOT NULL,
    sos        BIT           NOT NULL DEFAULT 0,
    user_id    INT           NULL,
    company_id INT           NULL,
    created_at DATETIME2     DEFAULT GETDATE()
);
GO

-- ── device_clock_alarms ───────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_clock_alarms')
CREATE TABLE device_clock_alarms (
    id         INT IDENTITY(1,1) PRIMARY KEY,
    device_id  NVARCHAR(50)  NOT NULL,
    repeat     BIT           NOT NULL DEFAULT 0,
    monday     BIT           NOT NULL DEFAULT 0,
    tuesday    BIT           NOT NULL DEFAULT 0,
    wednesday  BIT           NOT NULL DEFAULT 0,
    thursday   BIT           NOT NULL DEFAULT 0,
    friday     BIT           NOT NULL DEFAULT 0,
    saturday   BIT           NOT NULL DEFAULT 0,
    sunday     BIT           NOT NULL DEFAULT 0,
    hour       INT           NOT NULL,
    minute     INT           NOT NULL,
    title      NVARCHAR(100) NULL,
    created_at DATETIME2     DEFAULT GETDATE()
);
GO

-- ── device_sedentary ──────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_sedentary')
CREATE TABLE device_sedentary (
    id         INT IDENTITY(1,1) PRIMARY KEY,
    device_id  NVARCHAR(50) NOT NULL,
    repeat     BIT          NOT NULL DEFAULT 0,
    monday     BIT          NOT NULL DEFAULT 0,
    tuesday    BIT          NOT NULL DEFAULT 0,
    wednesday  BIT          NOT NULL DEFAULT 0,
    thursday   BIT          NOT NULL DEFAULT 0,
    friday     BIT          NOT NULL DEFAULT 0,
    saturday   BIT          NOT NULL DEFAULT 0,
    sunday     BIT          NOT NULL DEFAULT 0,
    start_hour INT          NOT NULL,
    end_hour   INT          NOT NULL,
    duration   INT          NOT NULL,
    threshold  INT          NOT NULL DEFAULT 40,
    user_id    INT          NULL,
    company_id INT          NULL,
    created_at DATETIME2    DEFAULT GETDATE()
);
GO

-- ── sleep_calculations ────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'sleep_calculations')
CREATE TABLE sleep_calculations (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    device_id   NVARCHAR(50)  NOT NULL,
    record_date NVARCHAR(10)  NOT NULL,
    completed   INT           NOT NULL,
    start_time  NVARCHAR(30)  NULL,
    end_time    NVARCHAR(30)  NULL,
    hr          INT           NULL,
    turn_times  INT           NULL,
    resp_avg    FLOAT         NULL,
    resp_max    FLOAT         NULL,
    resp_min    FLOAT         NULL,
    sections    NVARCHAR(MAX) NULL,
    user_id     INT           NULL,
    company_id  INT           NULL,
    created_at  DATETIME2     DEFAULT GETDATE()
);
GO

-- ── ecg_calculations ──────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ecg_calculations')
CREATE TABLE ecg_calculations (
    id         INT IDENTITY(1,1) PRIMARY KEY,
    device_id  NVARCHAR(50) NOT NULL,
    result     INT          NOT NULL,
    hr         INT          NOT NULL,
    effective  INT          NOT NULL,
    direction  INT          NOT NULL,
    user_id    INT          NULL,
    company_id INT          NULL,
    created_at DATETIME2    DEFAULT GETDATE()
);
GO

-- ── af_calculations ───────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'af_calculations')
CREATE TABLE af_calculations (
    id         INT IDENTITY(1,1) PRIMARY KEY,
    device_id  NVARCHAR(50) NOT NULL,
    result     INT          NOT NULL,
    user_id    INT          NULL,
    company_id INT          NULL,
    created_at DATETIME2    DEFAULT GETDATE()
);
GO

-- ── spo2_calculations ─────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'spo2_calculations')
CREATE TABLE spo2_calculations (
    id         INT IDENTITY(1,1) PRIMARY KEY,
    device_id  NVARCHAR(50) NOT NULL,
    spo2_score FLOAT        NOT NULL,
    osahs_risk INT          NULL,
    user_id    INT          NULL,
    company_id INT          NULL,
    created_at DATETIME2    DEFAULT GETDATE()
);
GO

-- =============================================================================
-- Indexes (idempotent)
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_gps_device_id' AND object_id=OBJECT_ID('gps_tracks'))
    CREATE INDEX IX_gps_device_id ON gps_tracks (device_id, id DESC)
        INCLUDE (gnss_time, longitude, latitude, loc_type, created_at);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_gps_device_created' AND object_id=OBJECT_ID('gps_tracks'))
    CREATE INDEX IX_gps_device_created ON gps_tracks (device_id, created_at ASC)
        INCLUDE (gnss_time, longitude, latitude, loc_type, id);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_health_device_id' AND object_id=OBJECT_ID('health_snapshots'))
    CREATE INDEX IX_health_device_id ON health_snapshots (device_id, id DESC)
        INCLUDE (record_time, battery, rssi, steps, distance, calorie,
                 avg_hr, max_hr, min_hr, avg_spo2, sbp, dbp, fatigue, created_at);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_alarms_created_at' AND object_id=OBJECT_ID('alarms'))
    CREATE INDEX IX_alarms_created_at ON alarms (created_at DESC)
        INCLUDE (device_id, alarm_time, alarm_type, details);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_sos_created_at' AND object_id=OBJECT_ID('sos_events'))
    CREATE INDEX IX_sos_created_at ON sos_events (created_at DESC);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_user_profiles_active_name' AND object_id=OBJECT_ID('user_profiles'))
    CREATE INDEX IX_user_profiles_active_name ON user_profiles (is_active, surname, name)
        INCLUDE (email, cell, emp_no, address, updated_at);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UQ_phonebook_device_number' AND object_id=OBJECT_ID('device_phonebook'))
    ALTER TABLE device_phonebook ADD CONSTRAINT UQ_phonebook_device_number UNIQUE (device_id, number);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UQ_clock_alarm_device_time' AND object_id=OBJECT_ID('device_clock_alarms'))
    ALTER TABLE device_clock_alarms ADD CONSTRAINT UQ_clock_alarm_device_time UNIQUE (device_id, hour, minute);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UQ_sedentary_device_window' AND object_id=OBJECT_ID('device_sedentary'))
    ALTER TABLE device_sedentary ADD CONSTRAINT UQ_sedentary_device_window UNIQUE (device_id, start_hour, end_hour);
GO

-- =============================================================================
-- Add user_id / company_id columns to any table that is missing them
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
GO

-- =============================================================================
-- Verification: list every table and its column count
-- =============================================================================
SELECT
    t.name                          AS table_name,
    COUNT(c.column_id)              AS column_count
FROM sys.tables t
JOIN sys.columns c ON c.object_id = t.object_id
WHERE t.type = 'U'
GROUP BY t.name
ORDER BY t.name;
