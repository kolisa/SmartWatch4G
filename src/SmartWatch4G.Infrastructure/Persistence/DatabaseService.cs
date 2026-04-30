using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces;

namespace SmartWatch4G.Infrastructure.Persistence;

public class DatabaseService : IDatabaseService, IDisposable
{
    private readonly string _connStr;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(IConfiguration config, ILogger<DatabaseService> logger)
    {
        _logger  = logger;
        _connStr = config.GetConnectionString("SmartWatch")
            ?? throw new InvalidOperationException("Connection string 'SmartWatch' not found.");

        InitializeSchema();
    }

    private SqlConnection Open() => new(_connStr);

    private async Task<SqlConnection> OpenAsync()
    {
        var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        return conn;
    }

    private string MasterConnStr()
    {
        var b = new SqlConnectionStringBuilder(_connStr) { InitialCatalog = "master" };
        if (!b.DataSource.StartsWith("tcp:", StringComparison.OrdinalIgnoreCase))
            b.DataSource = "tcp:" + b.DataSource;
        return b.ConnectionString;
    }

    private void EnsureDatabase()
    {
        var dbName = new SqlConnectionStringBuilder(_connStr).InitialCatalog;
        if (string.IsNullOrWhiteSpace(dbName) ||
            dbName.Equals("master", StringComparison.OrdinalIgnoreCase))
            return;

        try
        {
            using var conn = new SqlConnection(MasterConnStr());
            conn.Open();
            using var cmd = new SqlCommand(
                $"IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'{dbName}') CREATE DATABASE [{dbName}];",
                conn);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EnsureDatabase failed for '{Database}'.", dbName);
        }
    }

    private void InitializeSchema()
    {
        try
        {
            using var conn = Open();
            conn.Open();
            var ddl = @"
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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_fall_settings')
CREATE TABLE device_fall_settings (
    device_id       NVARCHAR(50) PRIMARY KEY,
    fall_check      BIT          NULL,
    fall_threshold  INT          NULL,
    user_id         INT          NULL,
    company_id      INT          NULL,
    updated_at      DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_data_freq')
CREATE TABLE device_data_freq (
    device_id          NVARCHAR(50) PRIMARY KEY,
    gps_auto_check     BIT          NULL,
    gps_interval_time  INT          NULL,
    power_mode         INT          NULL,
    user_id            INT          NULL,
    company_id         INT          NULL,
    updated_at         DATETIME2    DEFAULT GETDATE()
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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_clock_alarms')
CREATE TABLE device_clock_alarms (
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
    hour       INT          NOT NULL,
    minute     INT          NOT NULL,
    title      NVARCHAR(100) NULL,
    user_id    INT          NULL,
    company_id INT          NULL,
    created_at DATETIME2    DEFAULT GETDATE()
);

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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'sleep_calculations')
CREATE TABLE sleep_calculations (
    id            INT IDENTITY(1,1) PRIMARY KEY,
    device_id     NVARCHAR(50)  NOT NULL,
    record_date   NVARCHAR(10)  NOT NULL,
    completed     INT           NOT NULL,
    start_time    NVARCHAR(30)  NULL,
    end_time      NVARCHAR(30)  NULL,
    hr            INT           NULL,
    turn_times    INT           NULL,
    resp_avg      FLOAT         NULL,
    resp_max      FLOAT         NULL,
    resp_min      FLOAT         NULL,
    sections      NVARCHAR(MAX) NULL,
    deep_sleep    INT           NULL,
    light_sleep   INT           NULL,
    weak_sleep    INT           NULL,
    eyemove_sleep INT           NULL,
    user_id       INT           NULL,
    company_id    INT           NULL,
    created_at    DATETIME2     DEFAULT GETDATE()
);
IF COL_LENGTH('sleep_calculations','deep_sleep')    IS NULL ALTER TABLE sleep_calculations ADD deep_sleep    INT NULL;
IF COL_LENGTH('sleep_calculations','light_sleep')   IS NULL ALTER TABLE sleep_calculations ADD light_sleep   INT NULL;
IF COL_LENGTH('sleep_calculations','weak_sleep')    IS NULL ALTER TABLE sleep_calculations ADD weak_sleep    INT NULL;
IF COL_LENGTH('sleep_calculations','eyemove_sleep') IS NULL ALTER TABLE sleep_calculations ADD eyemove_sleep INT NULL;

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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'af_calculations')
CREATE TABLE af_calculations (
    id         INT IDENTITY(1,1) PRIMARY KEY,
    device_id  NVARCHAR(50) NOT NULL,
    result     INT          NOT NULL,
    user_id    INT          NULL,
    company_id INT          NULL,
    created_at DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'spo2_calculations')
CREATE TABLE spo2_calculations (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    device_id   NVARCHAR(50) NOT NULL,
    spo2_score  FLOAT        NOT NULL,
    osahs_risk  INT          NULL,
    user_id     INT          NULL,
    company_id  INT          NULL,
    created_at  DATETIME2    DEFAULT GETDATE()
);

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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'audit_log')
CREATE TABLE audit_log (
    id          BIGINT IDENTITY(1,1) PRIMARY KEY,
    action      NVARCHAR(20)   NOT NULL,
    table_name  NVARCHAR(100)  NOT NULL,
    device_id   NVARCHAR(50)   NULL,
    details     NVARCHAR(500)  NULL,
    occurred_at DATETIME2      NOT NULL DEFAULT GETDATE()
);

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
    IF COL_LENGTH('user_profiles', 'is_active') IS NULL
        ALTER TABLE user_profiles ADD is_active BIT NOT NULL DEFAULT 1;
    IF COL_LENGTH('user_profiles', 'user_id') IS NULL
        ALTER TABLE user_profiles ADD user_id INT IDENTITY(1,1) NOT NULL;
    IF COL_LENGTH('user_profiles', 'company_id') IS NULL
    BEGIN
        ALTER TABLE user_profiles ADD company_id INT NULL;
        IF OBJECT_ID('FK_user_profiles_company_id', 'F') IS NULL
            ALTER TABLE user_profiles
                ADD CONSTRAINT FK_user_profiles_company_id
                FOREIGN KEY (company_id) REFERENCES companies(id) ON DELETE SET NULL;
    END
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='uq_user_profiles_user_id'
                   AND object_id=OBJECT_ID('user_profiles'))
        ALTER TABLE user_profiles ADD CONSTRAINT uq_user_profiles_user_id UNIQUE (user_id);
END";
            using (var tableCmd = new SqlCommand(ddl, conn))
                tableCmd.ExecuteNonQuery();

            // ── Indexes (separate batch so all tables are guaranteed to exist) ──────
            const string indexDdl = @"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_audit_occurred'
               AND object_id=OBJECT_ID('audit_log'))
    CREATE INDEX IX_audit_occurred ON audit_log (occurred_at DESC)
        INCLUDE (action, table_name, device_id);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_gps_device_id'
               AND object_id=OBJECT_ID('gps_tracks'))
    CREATE INDEX IX_gps_device_id
        ON gps_tracks (device_id, id DESC)
        INCLUDE (gnss_time, longitude, latitude, loc_type, created_at);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_gps_device_created'
               AND object_id=OBJECT_ID('gps_tracks'))
    CREATE INDEX IX_gps_device_created
        ON gps_tracks (device_id, created_at ASC)
        INCLUDE (gnss_time, longitude, latitude, loc_type, id);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_health_device_id'
               AND object_id=OBJECT_ID('health_snapshots'))
    CREATE INDEX IX_health_device_id
        ON health_snapshots (device_id, id DESC)
        INCLUDE (record_time, battery, rssi, steps, distance, calorie,
                 avg_hr, max_hr, min_hr, avg_spo2, sbp, dbp, fatigue, created_at);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_alarms_created_at'
               AND object_id=OBJECT_ID('alarms'))
    CREATE INDEX IX_alarms_created_at
        ON alarms (created_at DESC)
        INCLUDE (device_id, alarm_time, alarm_type, details);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_sos_created_at'
               AND object_id=OBJECT_ID('sos_events'))
    CREATE INDEX IX_sos_created_at
        ON sos_events (created_at DESC);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_user_profiles_active_name'
               AND object_id=OBJECT_ID('user_profiles'))
    CREATE INDEX IX_user_profiles_active_name
        ON user_profiles (is_active, surname, name)
        INCLUDE (email, cell, emp_no, address, updated_at);

-- Unique constraints on multi-row-per-device tables to prevent duplicate entries
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UQ_phonebook_device_number'
               AND object_id=OBJECT_ID('device_phonebook'))
    ALTER TABLE device_phonebook
        ADD CONSTRAINT UQ_phonebook_device_number UNIQUE (device_id, number);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UQ_clock_alarm_device_time'
               AND object_id=OBJECT_ID('device_clock_alarms'))
    ALTER TABLE device_clock_alarms
        ADD CONSTRAINT UQ_clock_alarm_device_time UNIQUE (device_id, hour, minute);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UQ_sedentary_device_window'
               AND object_id=OBJECT_ID('device_sedentary'))
    ALTER TABLE device_sedentary
        ADD CONSTRAINT UQ_sedentary_device_window UNIQUE (device_id, start_hour, end_hour);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_sleep_device_date'
               AND object_id=OBJECT_ID('sleep_calculations'))
    CREATE INDEX IX_sleep_device_date
        ON sleep_calculations (device_id, record_date)
        INCLUDE (created_at);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_alarms_type_created'
               AND object_id=OBJECT_ID('alarms'))
    CREATE INDEX IX_alarms_type_created
        ON alarms (alarm_type, created_at DESC)
        INCLUDE (device_id, company_id);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_sos_company_created'
               AND object_id=OBJECT_ID('sos_events'))
    CREATE INDEX IX_sos_company_created
        ON sos_events (company_id, created_at DESC)
        INCLUDE (device_id);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_gps_company_created'
               AND object_id=OBJECT_ID('gps_tracks'))
    CREATE INDEX IX_gps_company_created
        ON gps_tracks (company_id, created_at DESC)
        INCLUDE (device_id);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_user_profiles_company_active'
               AND object_id=OBJECT_ID('user_profiles'))
    CREATE INDEX IX_user_profiles_company_active
        ON user_profiles (company_id, is_active)
        INCLUDE (user_id, name, surname);

-- ── Add user_id / company_id to existing tables (idempotent) ─────────────────
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
    SET @pos  = CHARINDEX(',', @tables);
    IF @pos   = 0 SET @pos = LEN(@tables) + 1;
    SET @tbl  = LEFT(@tables, @pos - 1);
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
";
            using (var idxCmd = new SqlCommand(indexDdl, conn))
                idxCmd.ExecuteNonQuery();

            // ── Seed default settings for known devices ───────────────────────
            var seedSql = @"
DECLARE @devices TABLE (device_id NVARCHAR(50));
INSERT INTO @devices VALUES
    ('863758060986873'),('863758060926292'),('863758060956587'),
    ('863758060926754'),('863758060987517'),('863758060987855'),
    ('863758060927422'),('863758060926499'),('863758060927455'),
    ('863758060982484'),('863758060926564'),('863758060987483'),
    ('863758060927232');

INSERT INTO device_locate_freq (device_id,data_auto_upload,data_upload_interval,auto_locate,locate_interval_time,power_mode,user_id,company_id)
SELECT d.device_id,1,300,1,60,2,u.user_id,1 FROM @devices d
LEFT JOIN user_profiles u ON u.device_id=d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_locate_freq t WHERE t.device_id=d.device_id);

INSERT INTO device_hr_alarm (device_id,[open],high,low,threshold,alarm_interval,user_id,company_id)
SELECT d.device_id,1,160,45,5,5,u.user_id,1 FROM @devices d
LEFT JOIN user_profiles u ON u.device_id=d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_hr_alarm t WHERE t.device_id=d.device_id);

INSERT INTO device_dynamic_hr_alarm (device_id,[open],high,low,timeout,interval,user_id,company_id)
SELECT d.device_id,0,160,45,30,5,u.user_id,1 FROM @devices d
LEFT JOIN user_profiles u ON u.device_id=d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_dynamic_hr_alarm t WHERE t.device_id=d.device_id);

INSERT INTO device_spo2_alarm (device_id,[open],low,user_id,company_id)
SELECT d.device_id,1,90,u.user_id,1 FROM @devices d
LEFT JOIN user_profiles u ON u.device_id=d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_spo2_alarm t WHERE t.device_id=d.device_id);

INSERT INTO device_bp_alarm (device_id,[open],sbp_high,sbp_below,dbp_high,dbp_below,user_id,company_id)
SELECT d.device_id,0,160,90,100,60,u.user_id,1 FROM @devices d
LEFT JOIN user_profiles u ON u.device_id=d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_bp_alarm t WHERE t.device_id=d.device_id);

INSERT INTO device_temp_alarm (device_id,[open],high,low,user_id,company_id)
SELECT d.device_id,0,39,35,u.user_id,1 FROM @devices d
LEFT JOIN user_profiles u ON u.device_id=d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_temp_alarm t WHERE t.device_id=d.device_id);

INSERT INTO device_fall_settings (device_id,fall_check,fall_threshold,user_id,company_id)
SELECT d.device_id,1,3,u.user_id,1 FROM @devices d
LEFT JOIN user_profiles u ON u.device_id=d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_fall_settings t WHERE t.device_id=d.device_id);

INSERT INTO device_hr_interval (device_id,interval,user_id,company_id)
SELECT d.device_id,5,u.user_id,1 FROM @devices d
LEFT JOIN user_profiles u ON u.device_id=d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_hr_interval t WHERE t.device_id=d.device_id);

INSERT INTO device_other_interval (device_id,interval,user_id,company_id)
SELECT d.device_id,10,u.user_id,1 FROM @devices d
LEFT JOIN user_profiles u ON u.device_id=d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_other_interval t WHERE t.device_id=d.device_id);

INSERT INTO device_goal (device_id,step,distance,calorie,user_id,company_id)
SELECT d.device_id,10000,5,400,u.user_id,1 FROM @devices d
LEFT JOIN user_profiles u ON u.device_id=d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_goal t WHERE t.device_id=d.device_id);

INSERT INTO device_display (device_id,language,hour_format,date_format,distance_unit,temperature_unit,wear_hand_right,user_id,company_id)
SELECT d.device_id,0,24,0,0,0,0,u.user_id,1 FROM @devices d
LEFT JOIN user_profiles u ON u.device_id=d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_display t WHERE t.device_id=d.device_id);

INSERT INTO device_auto_af (device_id,[open],interval,rri_single_time,rri_type,user_id,company_id)
SELECT d.device_id,0,60,0,0,u.user_id,1 FROM @devices d
LEFT JOIN user_profiles u ON u.device_id=d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_auto_af t WHERE t.device_id=d.device_id);

INSERT INTO device_user_info (device_id,user_id,company_id)
SELECT d.device_id,u.user_id,1 FROM @devices d
LEFT JOIN user_profiles u ON u.device_id=d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_user_info t WHERE t.device_id=d.device_id);

INSERT INTO device_bp_adjust (device_id,user_id,company_id)
SELECT d.device_id,u.user_id,1 FROM @devices d
LEFT JOIN user_profiles u ON u.device_id=d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_bp_adjust t WHERE t.device_id=d.device_id);
";
            using (var seedCmd = new SqlCommand(seedSql, conn))
                seedCmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "InitializeSchema failed.");
        }
    }

    public async Task InsertGpsTrack(string deviceId, string gnssTime, double longitude, double latitude, string locType)
    {
        deviceId = deviceId.Trim();
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                INSERT INTO gps_tracks (device_id, gnss_time, longitude, latitude, loc_type, user_id, company_id)
                SELECT @dev, @t, @lon, @lat, @type, u.user_id, u.company_id
                FROM (VALUES(1)) AS x(dummy)
                LEFT JOIN user_profiles u ON u.device_id=@dev AND u.is_active=1
                WHERE NOT EXISTS (
                    SELECT 1 FROM gps_tracks
                    WHERE device_id=@dev AND gnss_time=@t AND longitude=@lon AND latitude=@lat)", conn);
            cmd.Parameters.AddWithValue("@dev",  deviceId);
            cmd.Parameters.AddWithValue("@t",    gnssTime);
            cmd.Parameters.AddWithValue("@lon",  longitude);
            cmd.Parameters.AddWithValue("@lat",  latitude);
            cmd.Parameters.AddWithValue("@type", (object?)locType ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
            await LogAuditAsync(conn, "INSERT", "gps_tracks", deviceId, $"time:{gnssTime},type:{locType}");
        }
        catch (Exception ex) { _logger.LogError(ex, "InsertGpsTrack failed for {Device}", deviceId); }
    }

    public async Task UpsertHealthSnapshot(string deviceId, string recordTime,
        int? battery = null, int? rssi = null,
        int? steps = null, double? distance = null, double? calorie = null,
        int? avgHr = null, int? maxHr = null, int? minHr = null,
        int? avgSpo2 = null, int? sbp = null, int? dbp = null, int? fatigue = null,
        double? bodyTempEvi = null, int? bodyTempEsti = null, int? tempType = null,
        int? bpBpm = null, double? bloodPotassium = null, double? bloodSugar = null,
        double? biozR = null, double? biozX = null, double? biozFat = null,
        double? biozBmi = null, int? biozType = null,
        double? breathRate = null, int? moodLevel = null)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                MERGE health_snapshots WITH (HOLDLOCK) AS t
                USING (
                    SELECT @dev AS device_id, @rt AS record_time, u.user_id, u.company_id
                    FROM (VALUES(1)) AS x(dummy)
                    LEFT JOIN user_profiles u ON u.device_id=@dev AND u.is_active=1
                ) AS s ON t.device_id = s.device_id AND t.record_time = s.record_time
                WHEN MATCHED THEN UPDATE SET
                    battery         = COALESCE(@bat,  t.battery),
                    rssi            = COALESCE(@rssi, t.rssi),
                    steps           = COALESCE(@stp,  t.steps),
                    distance        = COALESCE(@dist, t.distance),
                    calorie         = COALESCE(@cal,  t.calorie),
                    avg_hr          = COALESCE(@ahr,  t.avg_hr),
                    max_hr          = COALESCE(@xhr,  t.max_hr),
                    min_hr          = COALESCE(@nhr,  t.min_hr),
                    avg_spo2        = COALESCE(@spo,  t.avg_spo2),
                    sbp             = COALESCE(@sbp,  t.sbp),
                    dbp             = COALESCE(@dbp,  t.dbp),
                    fatigue         = COALESCE(@fat,  t.fatigue),
                    body_temp_evi   = COALESCE(@bte,  t.body_temp_evi),
                    body_temp_esti  = COALESCE(@bts,  t.body_temp_esti),
                    temp_type       = COALESCE(@tty,  t.temp_type),
                    bp_bpm          = COALESCE(@bbp,  t.bp_bpm),
                    blood_potassium = COALESCE(@bpk,  t.blood_potassium),
                    blood_sugar     = COALESCE(@bsg,  t.blood_sugar),
                    bioz_r          = COALESCE(@bzr,  t.bioz_r),
                    bioz_x          = COALESCE(@bzx,  t.bioz_x),
                    bioz_fat        = COALESCE(@bzf,  t.bioz_fat),
                    bioz_bmi        = COALESCE(@bzm,  t.bioz_bmi),
                    bioz_type       = COALESCE(@bzt,  t.bioz_type),
                    breath_rate     = COALESCE(@brr,  t.breath_rate),
                    mood_level      = COALESCE(@mld,  t.mood_level),
                    user_id         = COALESCE(s.user_id,    t.user_id),
                    company_id      = COALESCE(s.company_id, t.company_id)
                WHEN NOT MATCHED THEN INSERT
                    (device_id, record_time,
                     battery, rssi, steps, distance, calorie,
                     avg_hr, max_hr, min_hr, avg_spo2, sbp, dbp, fatigue,
                     body_temp_evi, body_temp_esti, temp_type,
                     bp_bpm, blood_potassium, blood_sugar,
                     bioz_r, bioz_x, bioz_fat, bioz_bmi, bioz_type,
                     breath_rate, mood_level,
                     user_id, company_id)
                VALUES (s.device_id, s.record_time,
                    @bat, @rssi, @stp, @dist, @cal,
                    @ahr, @xhr, @nhr, @spo, @sbp, @dbp, @fat,
                    @bte, @bts, @tty, @bbp, @bpk, @bsg,
                    @bzr, @bzx, @bzf, @bzm, @bzt,
                    @brr, @mld,
                    s.user_id, s.company_id);", conn);

            cmd.Parameters.AddWithValue("@dev",  deviceId);
            cmd.Parameters.AddWithValue("@rt",   recordTime);
            cmd.Parameters.AddWithValue("@bat",  (object?)battery         ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@rssi", (object?)rssi            ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@stp",  (object?)steps           ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dist", (object?)distance        ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cal",  (object?)calorie         ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ahr",  (object?)avgHr           ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@xhr",  (object?)maxHr           ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@nhr",  (object?)minHr           ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@spo",  (object?)avgSpo2         ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@sbp",  (object?)sbp             ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dbp",  (object?)dbp             ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@fat",  (object?)fatigue         ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@bte",  (object?)bodyTempEvi     ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@bts",  (object?)bodyTempEsti    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tty",  (object?)tempType        ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@bbp",  (object?)bpBpm           ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@bpk",  (object?)bloodPotassium  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@bsg",  (object?)bloodSugar      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@bzr",  (object?)biozR           ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@bzx",  (object?)biozX           ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@bzf",  (object?)biozFat         ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@bzm",  (object?)biozBmi         ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@bzt",  (object?)biozType        ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@brr",  (object?)breathRate      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@mld",  (object?)moodLevel       ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
            await LogAuditAsync(conn, "UPSERT", "health_snapshots", deviceId, recordTime);
        }
        catch (Exception ex) { _logger.LogError(ex, "UpsertHealthSnapshot failed for {Device}", deviceId); }
    }

    public async Task InsertAlarm(string deviceId, string alarmTime, string alarmType, string? details = null)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                INSERT INTO alarms (device_id, alarm_time, alarm_type, details, user_id, company_id)
                SELECT @dev, @t, @type, @det, u.user_id, u.company_id
                FROM (VALUES(1)) AS x(dummy)
                LEFT JOIN user_profiles u ON u.device_id=@dev AND u.is_active=1
                WHERE NOT EXISTS (
                    SELECT 1 FROM alarms
                    WHERE device_id=@dev AND alarm_time=@t AND alarm_type=@type)", conn);
            cmd.Parameters.AddWithValue("@dev",  deviceId);
            cmd.Parameters.AddWithValue("@t",    alarmTime);
            cmd.Parameters.AddWithValue("@type", alarmType);
            cmd.Parameters.AddWithValue("@det",  (object?)details ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
            await LogAuditAsync(conn, "INSERT", "alarms", deviceId, $"type:{alarmType}");
        }
        catch (Exception ex) { _logger.LogError(ex, "InsertAlarm failed for {Device}", deviceId); }
    }

    public async Task InsertSosEvent(string deviceId, string alarmTime,
        double? lat, double? lon,
        string? callNumber, int? callStatus, string? callStart, string? callEnd)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                INSERT INTO sos_events (device_id, alarm_time, latitude, longitude, call_number, call_status, call_start, call_end, user_id, company_id)
                SELECT @dev, @t, @lat, @lon, @num, @status, @start, @end, u.user_id, u.company_id
                FROM (VALUES(1)) AS x(dummy)
                LEFT JOIN user_profiles u ON u.device_id=@dev AND u.is_active=1
                WHERE NOT EXISTS (
                    SELECT 1 FROM sos_events WHERE device_id=@dev AND alarm_time=@t)", conn);
            cmd.Parameters.AddWithValue("@dev",    deviceId);
            cmd.Parameters.AddWithValue("@t",      alarmTime);
            cmd.Parameters.AddWithValue("@lat",    (object?)lat        ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@lon",    (object?)lon        ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@num",    (object?)callNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@status", (object?)callStatus ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@start",  (object?)callStart  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@end",    (object?)callEnd    ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
            await LogAuditAsync(conn, "INSERT", "sos_events", deviceId, alarmTime);
        }
        catch (Exception ex) { _logger.LogError(ex, "InsertSosEvent failed for {Device}", deviceId); }
    }

    public async Task InsertDeviceInfo(string deviceId, string recordedAt,
        string? model, string? version, string? wearingStatus, string? signal, string rawJson)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                INSERT INTO device_info_log (device_id, recorded_at, model, version, wearing_status, signal, raw_json, user_id, company_id)
                SELECT @dev, @rat, @model, @ver, @wear, @sig, @json, u.user_id, u.company_id
                FROM (VALUES(1)) AS x(dummy)
                LEFT JOIN user_profiles u ON u.device_id=@dev AND u.is_active=1", conn);
            cmd.Parameters.AddWithValue("@dev",   deviceId);
            cmd.Parameters.AddWithValue("@rat",   recordedAt);
            cmd.Parameters.AddWithValue("@model", (object?)model         ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ver",   (object?)version       ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@wear",  (object?)wearingStatus ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@sig",   (object?)signal        ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@json",  rawJson);
            await cmd.ExecuteNonQueryAsync();
            await LogAuditAsync(conn, "INSERT", "device_info_log", deviceId, $"model:{model},ver:{version}");
        }
        catch (Exception ex) { _logger.LogError(ex, "InsertDeviceInfo failed for {Device}", deviceId); }
    }

    public async Task InsertSleepCalculation(string deviceId, string recordDate,
        int completed, string? startTime, string? endTime, int hr, int turnTimes,
        double? respAvg, double? respMax, double? respMin, string? sectionsJson,
        int? deepSleep = null, int? lightSleep = null, int? weakSleep = null, int? eyemoveSleep = null)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                INSERT INTO sleep_calculations
                    (device_id,record_date,completed,start_time,end_time,hr,turn_times,
                     resp_avg,resp_max,resp_min,sections,
                     deep_sleep,light_sleep,weak_sleep,eyemove_sleep,
                     user_id,company_id)
                SELECT @dev,@rd,@comp,@st,@et,@hr,@tt,
                       @ra,@rx,@rn,@sec,
                       @ds,@ls,@ws,@es,
                       u.user_id, u.company_id
                FROM (VALUES(1)) AS x(dummy)
                LEFT JOIN user_profiles u ON u.device_id=@dev AND u.is_active=1", conn);
            cmd.Parameters.AddWithValue("@dev",  deviceId);
            cmd.Parameters.AddWithValue("@rd",   recordDate);
            cmd.Parameters.AddWithValue("@comp", completed);
            cmd.Parameters.AddWithValue("@st",   (object?)startTime    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@et",   (object?)endTime      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@hr",   hr);
            cmd.Parameters.AddWithValue("@tt",   turnTimes);
            cmd.Parameters.AddWithValue("@ra",   (object?)respAvg      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@rx",   (object?)respMax      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@rn",   (object?)respMin      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@sec",  (object?)sectionsJson ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ds",   (object?)deepSleep    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ls",   (object?)lightSleep   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ws",   (object?)weakSleep    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@es",   (object?)eyemoveSleep ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
            await LogAuditAsync(conn, "INSERT", "sleep_calculations", deviceId, recordDate);
        }
        catch (Exception ex) { _logger.LogError(ex, "InsertSleepCalculation failed for {Device}", deviceId); }
    }

    public async Task<SleepCalculation?> GetSleepCalculation(string deviceId, string sleepDate)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                SELECT TOP 1
                    device_id, record_date, completed, start_time, end_time,
                    hr, turn_times, resp_avg, resp_max, resp_min, sections,
                    deep_sleep, light_sleep, weak_sleep, eyemove_sleep
                FROM sleep_calculations
                WHERE device_id = @dev AND record_date = @rd
                ORDER BY created_at DESC", conn);
            cmd.Parameters.AddWithValue("@dev", deviceId);
            cmd.Parameters.AddWithValue("@rd",  sleepDate);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;
            return new SleepCalculation
            {
                DeviceId    = reader.GetString(0),
                RecordDate  = reader.GetString(1),
                Completed   = reader.GetInt32(2),
                StartTime   = reader.IsDBNull(3)  ? null : reader.GetString(3),
                EndTime     = reader.IsDBNull(4)  ? null : reader.GetString(4),
                Hr          = reader.IsDBNull(5)  ? 0    : reader.GetInt32(5),
                TurnTimes   = reader.IsDBNull(6)  ? 0    : reader.GetInt32(6),
                RespAvg     = reader.IsDBNull(7)  ? null : reader.GetDouble(7),
                RespMax     = reader.IsDBNull(8)  ? null : reader.GetDouble(8),
                RespMin     = reader.IsDBNull(9)  ? null : reader.GetDouble(9),
                Sections    = reader.IsDBNull(10) ? null : reader.GetString(10),
                DeepSleep   = reader.IsDBNull(11) ? null : reader.GetInt32(11),
                LightSleep  = reader.IsDBNull(12) ? null : reader.GetInt32(12),
                WeakSleep   = reader.IsDBNull(13) ? null : reader.GetInt32(13),
                EyemoveSleep = reader.IsDBNull(14) ? null : reader.GetInt32(14),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSleepCalculation failed for {Device} {Date}", deviceId, sleepDate);
            return null;
        }
    }

    public async Task InsertEcgWaveform(string deviceId, string recordedAt, int sampleCount, string rawDataJson)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                INSERT INTO ecg_waveforms (device_id,recorded_at,sample_count,raw_data,user_id,company_id)
                SELECT @dev,@ra,@sc,@rd, u.user_id, u.company_id
                FROM (VALUES(1)) AS x(dummy)
                LEFT JOIN user_profiles u ON u.device_id=@dev AND u.is_active=1", conn);
            cmd.Parameters.AddWithValue("@dev", deviceId);
            cmd.Parameters.AddWithValue("@ra",  recordedAt);
            cmd.Parameters.AddWithValue("@sc",  sampleCount);
            cmd.Parameters.AddWithValue("@rd",  rawDataJson);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex) { _logger.LogError(ex, "InsertEcgWaveform failed for {Device}", deviceId); }
    }

    public async Task InsertPpgWaveform(string deviceId, string recordedAt, int sampleCount, string rawDataJson)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                INSERT INTO ppg_waveforms (device_id,recorded_at,sample_count,raw_data,user_id,company_id)
                SELECT @dev,@ra,@sc,@rd, u.user_id, u.company_id
                FROM (VALUES(1)) AS x(dummy)
                LEFT JOIN user_profiles u ON u.device_id=@dev AND u.is_active=1", conn);
            cmd.Parameters.AddWithValue("@dev", deviceId);
            cmd.Parameters.AddWithValue("@ra",  recordedAt);
            cmd.Parameters.AddWithValue("@sc",  sampleCount);
            cmd.Parameters.AddWithValue("@rd",  rawDataJson);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex) { _logger.LogError(ex, "InsertPpgWaveform failed for {Device}", deviceId); }
    }

    public async Task InsertAccWaveform(string deviceId, string recordedAt, int sampleCount,
        string? accXBase64, string? accYBase64, string? accZBase64)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                INSERT INTO acc_waveforms (device_id,recorded_at,sample_count,acc_x,acc_y,acc_z,user_id,company_id)
                SELECT @dev,@ra,@sc,@ax,@ay,@az, u.user_id, u.company_id
                FROM (VALUES(1)) AS x(dummy)
                LEFT JOIN user_profiles u ON u.device_id=@dev AND u.is_active=1", conn);
            cmd.Parameters.AddWithValue("@dev", deviceId);
            cmd.Parameters.AddWithValue("@ra",  recordedAt);
            cmd.Parameters.AddWithValue("@sc",  sampleCount);
            cmd.Parameters.AddWithValue("@ax",  (object?)accXBase64 ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ay",  (object?)accYBase64 ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@az",  (object?)accZBase64 ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex) { _logger.LogError(ex, "InsertAccWaveform failed for {Device}", deviceId); }
    }

    public async Task InsertRriWaveform(string deviceId, string recordedAt, int sampleCount, string rawDataJson)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                INSERT INTO rri_waveforms (device_id,recorded_at,sample_count,raw_data,user_id,company_id)
                SELECT @dev,@ra,@sc,@rd, u.user_id, u.company_id
                FROM (VALUES(1)) AS x(dummy)
                LEFT JOIN user_profiles u ON u.device_id=@dev AND u.is_active=1", conn);
            cmd.Parameters.AddWithValue("@dev", deviceId);
            cmd.Parameters.AddWithValue("@ra",  recordedAt);
            cmd.Parameters.AddWithValue("@sc",  sampleCount);
            cmd.Parameters.AddWithValue("@rd",  rawDataJson);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex) { _logger.LogError(ex, "InsertRriWaveform failed for {Device}", deviceId); }
    }

    public async Task InsertSpo2Waveform(string deviceId, string recordedAt, string readingsJson)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                INSERT INTO spo2_waveforms (device_id,recorded_at,readings,user_id,company_id)
                SELECT @dev,@ra,@rd, u.user_id, u.company_id
                FROM (VALUES(1)) AS x(dummy)
                LEFT JOIN user_profiles u ON u.device_id=@dev AND u.is_active=1", conn);
            cmd.Parameters.AddWithValue("@dev", deviceId);
            cmd.Parameters.AddWithValue("@ra",  recordedAt);
            cmd.Parameters.AddWithValue("@rd",  readingsJson);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex) { _logger.LogError(ex, "InsertSpo2Waveform failed for {Device}", deviceId); }
    }

    public async Task InsertMultiLeadsEcgWaveform(string deviceId, string recordedAt, int channels, int byteLen, string rawBase64)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                INSERT INTO multi_leads_ecg_waveforms (device_id,recorded_at,channels,byte_len,raw_data,user_id,company_id)
                SELECT @dev,@ra,@ch,@bl,@rd, u.user_id, u.company_id
                FROM (VALUES(1)) AS x(dummy)
                LEFT JOIN user_profiles u ON u.device_id=@dev AND u.is_active=1", conn);
            cmd.Parameters.AddWithValue("@dev", deviceId);
            cmd.Parameters.AddWithValue("@ra",  recordedAt);
            cmd.Parameters.AddWithValue("@ch",  channels);
            cmd.Parameters.AddWithValue("@bl",  byteLen);
            cmd.Parameters.AddWithValue("@rd",  rawBase64);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex) { _logger.LogError(ex, "InsertMultiLeadsEcgWaveform failed for {Device}", deviceId); }
    }

    public async Task InsertThirdPartyReading(string deviceId, string macAddr, string? devName, string readingType,
        string? recordedAt, double? sbp, double? dbp, double? hr, double? pulse,
        double? weight, double? impedance, double? bodyFatPct,
        double? spo2, double? pi, double? bodyTemp, double? value)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                INSERT INTO third_party_readings
                    (device_id,mac_addr,dev_name,reading_type,recorded_at,
                     sbp,dbp,hr,pulse,weight,impedance,body_fat_pct,spo2,pi,body_temp,value,
                     user_id,company_id)
                SELECT @dev,@mac,@dn,@rt,@ra,
                       @sbp,@dbp,@hr,@pul,@wgt,@imp,@bfp,@spo,@pi,@btm,@val,
                       u.user_id, u.company_id
                FROM (VALUES(1)) AS x(dummy)
                LEFT JOIN user_profiles u ON u.device_id=@dev AND u.is_active=1", conn);
            cmd.Parameters.AddWithValue("@dev", deviceId);
            cmd.Parameters.AddWithValue("@mac", macAddr);
            cmd.Parameters.AddWithValue("@dn",  (object?)devName    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@rt",  readingType);
            cmd.Parameters.AddWithValue("@ra",  (object?)recordedAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@sbp", (object?)sbp        ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dbp", (object?)dbp        ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@hr",  (object?)hr         ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@pul", (object?)pulse      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@wgt", (object?)weight     ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@imp", (object?)impedance  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@bfp", (object?)bodyFatPct ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@spo", (object?)spo2       ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@pi",  (object?)pi         ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@btm", (object?)bodyTemp   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@val", (object?)value      ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex) { _logger.LogError(ex, "InsertThirdPartyReading failed for {Device}", deviceId); }
    }

    public async Task InsertEcgCalculation(string deviceId, int result, int hr, int effective, int direction)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                INSERT INTO ecg_calculations (device_id,result,hr,effective,direction,user_id,company_id)
                SELECT @dev,@res,@hr,@eff,@dir, u.user_id, u.company_id
                FROM (VALUES(1)) AS x(dummy)
                LEFT JOIN user_profiles u ON u.device_id=@dev AND u.is_active=1", conn);
            cmd.Parameters.AddWithValue("@dev", deviceId);
            cmd.Parameters.AddWithValue("@res", result);
            cmd.Parameters.AddWithValue("@hr",  hr);
            cmd.Parameters.AddWithValue("@eff", effective);
            cmd.Parameters.AddWithValue("@dir", direction);
            await cmd.ExecuteNonQueryAsync();
            await LogAuditAsync(conn, "INSERT", "ecg_calculations", deviceId, $"result:{result}");
        }
        catch (Exception ex) { _logger.LogError(ex, "InsertEcgCalculation failed for {Device}", deviceId); }
    }

    public async Task InsertAfCalculation(string deviceId, int result)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                INSERT INTO af_calculations (device_id,result,user_id,company_id)
                SELECT @dev,@res, u.user_id, u.company_id
                FROM (VALUES(1)) AS x(dummy)
                LEFT JOIN user_profiles u ON u.device_id=@dev AND u.is_active=1", conn);
            cmd.Parameters.AddWithValue("@dev", deviceId);
            cmd.Parameters.AddWithValue("@res", result);
            await cmd.ExecuteNonQueryAsync();
            await LogAuditAsync(conn, "INSERT", "af_calculations", deviceId, $"result:{result}");
        }
        catch (Exception ex) { _logger.LogError(ex, "InsertAfCalculation failed for {Device}", deviceId); }
    }

    public async Task InsertSpo2Calculation(string deviceId, double spo2Score, int? oshahsRisk)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                INSERT INTO spo2_calculations (device_id,spo2_score,osahs_risk,user_id,company_id)
                SELECT @dev,@score,@risk, u.user_id, u.company_id
                FROM (VALUES(1)) AS x(dummy)
                LEFT JOIN user_profiles u ON u.device_id=@dev AND u.is_active=1", conn);
            cmd.Parameters.AddWithValue("@dev",   deviceId);
            cmd.Parameters.AddWithValue("@score", spo2Score);
            cmd.Parameters.AddWithValue("@risk",  (object?)oshahsRisk ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
            await LogAuditAsync(conn, "INSERT", "spo2_calculations", deviceId, $"score:{spo2Score}");
        }
        catch (Exception ex) { _logger.LogError(ex, "InsertSpo2Calculation failed for {Device}", deviceId); }
    }

    public async Task UpsertUserProfile(string deviceId, string name, string surname,
        string? email = null, string? cell = null, string? empNo = null, string? address = null,
        int? companyId = null)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                MERGE user_profiles AS t
                USING (SELECT @dev AS device_id) AS s ON t.device_id = s.device_id
                WHEN MATCHED THEN UPDATE SET
                    name       = @name,
                    surname    = @surname,
                    email      = @email,
                    cell       = @cell,
                    emp_no     = @empNo,
                    address    = @address,
                    company_id = COALESCE(@companyId, t.company_id),
                    is_active  = 1,
                    updated_at = GETDATE()
                WHEN NOT MATCHED THEN INSERT (device_id, name, surname, email, cell, emp_no, address, company_id, is_active)
                    VALUES (@dev, @name, @surname, @email, @cell, @empNo, @address, @companyId, 1);", conn);
            cmd.Parameters.AddWithValue("@dev",       deviceId);
            cmd.Parameters.AddWithValue("@name",      name);
            cmd.Parameters.AddWithValue("@surname",   surname);
            cmd.Parameters.AddWithValue("@email",     (object?)email     ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cell",      (object?)cell      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@empNo",     (object?)empNo     ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@address",   (object?)address   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@companyId", (object?)companyId ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
            await LogAuditAsync(conn, "UPSERT", "user_profiles", deviceId, $"{name} {surname}");
        }
        catch (Exception ex) { _logger.LogError(ex, "UpsertUserProfile failed for {Device}", deviceId); }
    }

    public async Task<UserProfile?> GetUserProfile(string deviceId)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                SELECT u.device_id, u.user_id, u.name, u.surname, u.email, u.cell,
                       u.emp_no, u.address, u.company_id, c.name AS company_name, u.updated_at
                FROM user_profiles u
                LEFT JOIN companies c ON c.id = u.company_id
                WHERE u.device_id = @dev AND u.is_active = 1", conn);
            cmd.Parameters.AddWithValue("@dev", deviceId);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;
            return MapUserProfile(reader);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserProfile failed for {Device}", deviceId);
            return null;
        }
    }

    public async Task<IReadOnlyList<UserProfile>> GetAllUserProfiles()
    {
        var list = new List<UserProfile>();
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                SELECT u.device_id, u.user_id, u.name, u.surname, u.email, u.cell,
                       u.emp_no, u.address, u.company_id, c.name AS company_name, u.updated_at
                FROM user_profiles u
                LEFT JOIN companies c ON c.id = u.company_id
                WHERE u.is_active = 1
                ORDER BY u.surname, u.name", conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                list.Add(MapUserProfile(reader));
        }
        catch (Exception ex) { _logger.LogError(ex, "GetAllUserProfiles failed."); }
        return list;
    }

    public async Task<IReadOnlyList<UserProfile>> GetUsersByCompanyId(int companyId)
    {
        var list = new List<UserProfile>();
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                SELECT u.device_id, u.user_id, u.name, u.surname, u.email, u.cell,
                       u.emp_no, u.address, u.company_id, c.name AS company_name, u.updated_at
                FROM user_profiles u
                LEFT JOIN companies c ON c.id = u.company_id
                WHERE u.is_active = 1 AND u.company_id = @companyId
                ORDER BY u.surname, u.name", conn);
            cmd.Parameters.AddWithValue("@companyId", companyId);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                list.Add(MapUserProfile(reader));
        }
        catch (Exception ex) { _logger.LogError(ex, "GetUsersByCompanyId failed for company {Id}", companyId); }
        return list;
    }

    public async Task ReactivateUserProfile(string deviceId)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                UPDATE user_profiles
                SET is_active = 1, updated_at = GETDATE()
                WHERE device_id = @dev AND is_active = 0", conn);
            cmd.Parameters.AddWithValue("@dev", deviceId);
            await cmd.ExecuteNonQueryAsync();
            await LogAuditAsync(conn, "UPDATE", "user_profiles", deviceId, "reactivated");
        }
        catch (Exception ex) { _logger.LogError(ex, "ReactivateUserProfile failed for {Device}", deviceId); }
    }

    private static UserProfile MapUserProfile(SqlDataReader r) => new()
    {
        DeviceId    = r.GetString(0),
        UserId      = r.GetInt32(1),
        Name        = r.GetString(2),
        Surname     = r.GetString(3),
        Email       = r.IsDBNull(4)  ? null : r.GetString(4),
        Cell        = r.IsDBNull(5)  ? null : r.GetString(5),
        EmpNo       = r.IsDBNull(6)  ? null : r.GetString(6),
        Address     = r.IsDBNull(7)  ? null : r.GetString(7),
        CompanyId   = r.IsDBNull(8)  ? null : r.GetInt32(8),
        CompanyName = r.IsDBNull(9)  ? null : r.GetString(9),
        UpdatedAt   = r.GetDateTime(10),
        IsActive    = true
    };

    public async Task DeleteUserProfile(string deviceId)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                UPDATE user_profiles
                SET is_active = 0, updated_at = GETDATE()
                WHERE device_id = @dev AND is_active = 1", conn);
            cmd.Parameters.AddWithValue("@dev", deviceId);
            await cmd.ExecuteNonQueryAsync();
            await LogAuditAsync(conn, "DELETE", "user_profiles", deviceId);
        }
        catch (Exception ex) { _logger.LogError(ex, "DeleteUserProfile failed for {Device}", deviceId); }
    }

    // ── Company CRUD ───────────────────────────────────────────────────────────

    public async Task<int> CreateCompany(string name, string? registrationNumber, string? contactEmail,
        string? contactPhone, string? address)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                INSERT INTO companies (name, registration_number, contact_email, contact_phone, address)
                OUTPUT INSERTED.id
                VALUES (@name, @reg, @email, @phone, @addr)", conn);
            cmd.Parameters.AddWithValue("@name",  name);
            cmd.Parameters.AddWithValue("@reg",   (object?)registrationNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@email", (object?)contactEmail       ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@phone", (object?)contactPhone       ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@addr",  (object?)address            ?? DBNull.Value);
            var newId = (int)(await cmd.ExecuteScalarAsync())!;
            await LogAuditAsync(conn, "INSERT", "companies", null, $"id:{newId},name:{name}");
            return newId;;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateCompany failed for '{Name}'", name);
            return -1;
        }
    }

    public async Task<Company?> GetCompany(int id)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                SELECT id, name, registration_number, contact_email, contact_phone,
                       address, is_active, created_at, updated_at
                FROM companies
                WHERE id = @id AND is_active = 1", conn);
            cmd.Parameters.AddWithValue("@id", id);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;
            return MapCompany(reader);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCompany failed for id={Id}", id);
            return null;
        }
    }

    public async Task<IReadOnlyList<Company>> GetAllCompanies()
    {
        var list = new List<Company>();
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                SELECT id, name, registration_number, contact_email, contact_phone,
                       address, is_active, created_at, updated_at
                FROM companies
                WHERE is_active = 1
                ORDER BY name", conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                list.Add(MapCompany(reader));
        }
        catch (Exception ex) { _logger.LogError(ex, "GetAllCompanies failed."); }
        return list;
    }

    private static Company MapCompany(SqlDataReader r) => new()
    {
        Id                 = r.GetInt32(0),
        Name               = r.GetString(1),
        RegistrationNumber = r.IsDBNull(2) ? null : r.GetString(2),
        ContactEmail       = r.IsDBNull(3) ? null : r.GetString(3),
        ContactPhone       = r.IsDBNull(4) ? null : r.GetString(4),
        Address            = r.IsDBNull(5) ? null : r.GetString(5),
        IsActive           = r.GetBoolean(6),
        CreatedAt          = r.GetDateTime(7),
        UpdatedAt          = r.GetDateTime(8)
    };

    public async Task UpdateCompany(int id, string name, string? registrationNumber, string? contactEmail,
        string? contactPhone, string? address)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                UPDATE companies
                SET name                = @name,
                    registration_number = @reg,
                    contact_email       = @email,
                    contact_phone       = @phone,
                    address             = @addr,
                    updated_at          = GETDATE()
                WHERE id = @id AND is_active = 1", conn);
            cmd.Parameters.AddWithValue("@id",    id);
            cmd.Parameters.AddWithValue("@name",  name);
            cmd.Parameters.AddWithValue("@reg",   (object?)registrationNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@email", (object?)contactEmail       ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@phone", (object?)contactPhone       ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@addr",  (object?)address            ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
            await LogAuditAsync(conn, "UPDATE", "companies", null, $"id:{id},name:{name}");
        }
        catch (Exception ex) { _logger.LogError(ex, "UpdateCompany failed for id={Id}", id); }
    }

    public async Task DeleteCompany(int id)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                UPDATE companies SET is_active = 0, updated_at = GETDATE() WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            await cmd.ExecuteNonQueryAsync();
            await LogAuditAsync(conn, "DELETE", "companies", null, $"id:{id}");
        }
        catch (Exception ex) { _logger.LogError(ex, "DeleteCompany failed for id={Id}", id); }
    }

    public async Task LinkUserToCompany(string deviceId, int? companyId)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                UPDATE user_profiles
                SET company_id = @companyId, updated_at = GETDATE()
                WHERE device_id = @dev AND is_active = 1", conn);
            cmd.Parameters.AddWithValue("@dev",       deviceId);
            cmd.Parameters.AddWithValue("@companyId", (object?)companyId ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
            await LogAuditAsync(conn, "UPDATE", "user_profiles", deviceId, $"companyId:{companyId}");
        }
        catch (Exception ex) { _logger.LogError(ex, "LinkUserToCompany failed for {Device}", deviceId); }
    }

    public async Task<int> BackfillDeviceRecords(string deviceId)
    {
        try
        {
            await using var conn = await OpenAsync();

            int? userId = null, companyId = null;
            using (var uCmd = new SqlCommand(
                "SELECT user_id, company_id FROM user_profiles WHERE device_id=@dev AND is_active=1", conn))
            {
                uCmd.Parameters.AddWithValue("@dev", deviceId);
                await using var rdr = await uCmd.ExecuteReaderAsync();
                if (await rdr.ReadAsync())
                {
                    userId    = rdr.IsDBNull(0) ? null : rdr.GetInt32(0);
                    companyId = rdr.IsDBNull(1) ? null : rdr.GetInt32(1);
                }
            }

            string[] tables =
            [
                "gps_tracks", "health_snapshots", "alarms", "sos_events", "device_info_log",
                "device_user_info", "device_fall_settings", "device_data_freq", "device_locate_freq",
                "device_lcd_gesture", "device_hr_alarm", "device_dynamic_hr_alarm", "device_spo2_alarm",
                "device_bp_alarm", "device_temp_alarm", "device_auto_af", "device_goal", "device_display",
                "device_bp_adjust", "device_hr_interval", "device_other_interval", "device_gps_settings",
                "device_phonebook", "device_clock_alarms", "device_sedentary",
                "sleep_calculations", "ecg_calculations", "af_calculations", "spo2_calculations",
                "ecg_waveforms", "ppg_waveforms", "acc_waveforms",
                "rri_waveforms", "spo2_waveforms", "multi_leads_ecg_waveforms", "third_party_readings"
            ];

            int total = 0;
            foreach (var tbl in tables)
            {
                using var upd = new SqlCommand(
                    $"UPDATE [{tbl}] SET user_id=@uid, company_id=@cid WHERE device_id=@dev", conn);
                upd.Parameters.AddWithValue("@dev", deviceId);
                upd.Parameters.AddWithValue("@uid", (object?)userId    ?? DBNull.Value);
                upd.Parameters.AddWithValue("@cid", (object?)companyId ?? DBNull.Value);
                total += await upd.ExecuteNonQueryAsync();
            }
            await LogAuditAsync(conn, "UPDATE", "backfill", deviceId, $"rows:{total}");
            return total;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BackfillDeviceRecords failed for {Device}", deviceId);
            return -1;
        }
    }

    public async Task<GnssTrack?> GetLatestGnssTrack(string deviceId)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                SELECT TOP 1 id, device_id, gnss_time, longitude, latitude, loc_type, created_at
                FROM gps_tracks
                WHERE device_id = @dev
                ORDER BY id DESC", conn);
            cmd.Parameters.AddWithValue("@dev", deviceId);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;
            return new GnssTrack
            {
                Id        = reader.GetInt32(0),
                DeviceId  = reader.GetString(1),
                GnssTime  = reader.GetString(2),
                Longitude = reader.GetDouble(3),
                Latitude  = reader.GetDouble(4),
                LocType   = reader.IsDBNull(5) ? null : reader.GetString(5),
                CreatedAt = reader.GetDateTime(6)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetLatestGnssTrack failed for {Device}", deviceId);
            return null;
        }
    }

    public async Task<IReadOnlyList<GnssTrack>> GetGnssTracks(string deviceId, System.DateTime? from, System.DateTime? to)
    {
        var list = new List<GnssTrack>();
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                SELECT id, device_id, gnss_time, longitude, latitude, loc_type, created_at
                FROM gps_tracks
                WHERE device_id = @dev
                  AND (@from IS NULL OR created_at >= @from)
                  AND (@to   IS NULL OR created_at <= @to)
                ORDER BY created_at DESC, id DESC", conn);
            cmd.Parameters.AddWithValue("@dev",  deviceId);
            cmd.Parameters.Add("@from", System.Data.SqlDbType.DateTime2).Value = (object?)from ?? DBNull.Value;
            cmd.Parameters.Add("@to",   System.Data.SqlDbType.DateTime2).Value = (object?)to   ?? DBNull.Value;
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new GnssTrack
                {
                    Id        = reader.GetInt32(0),
                    DeviceId  = reader.GetString(1),
                    GnssTime  = reader.GetString(2),
                    Longitude = reader.GetDouble(3),
                    Latitude  = reader.GetDouble(4),
                    LocType   = reader.IsDBNull(5) ? null : reader.GetString(5),
                    CreatedAt = reader.GetDateTime(6)
                });
            }
        }
        catch (Exception ex) { _logger.LogError(ex, "GetGnssTracks failed for {Device}", deviceId); }
        return list;
    }

    public async Task<HealthSnapshot?> GetLatestHealthSnapshot(string deviceId)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                SELECT TOP 1 id, device_id, record_time, battery, rssi, steps,
                       distance, calorie, avg_hr, max_hr, min_hr,
                       avg_spo2, sbp, dbp, fatigue,
                       body_temp_evi, body_temp_esti, temp_type, bp_bpm, blood_potassium, blood_sugar,
                       bioz_r, bioz_x, bioz_fat, bioz_bmi, bioz_type, breath_rate, mood_level,
                       created_at
                FROM health_snapshots
                WHERE device_id = @dev
                ORDER BY id DESC", conn);
            cmd.Parameters.AddWithValue("@dev", deviceId);
            await using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync()) return null;
            return new HealthSnapshot
            {
                Id              = r.GetInt32(0),
                DeviceId        = r.GetString(1),
                RecordTime      = r.GetString(2),
                Battery         = r.IsDBNull(3)  ? null : r.GetInt32(3),
                Rssi            = r.IsDBNull(4)  ? null : r.GetInt32(4),
                Steps           = r.IsDBNull(5)  ? null : r.GetInt32(5),
                Distance        = r.IsDBNull(6)  ? null : r.GetDouble(6),
                Calorie         = r.IsDBNull(7)  ? null : r.GetDouble(7),
                AvgHr           = r.IsDBNull(8)  ? null : r.GetInt32(8),
                MaxHr           = r.IsDBNull(9)  ? null : r.GetInt32(9),
                MinHr           = r.IsDBNull(10) ? null : r.GetInt32(10),
                AvgSpo2         = r.IsDBNull(11) ? null : r.GetInt32(11),
                Sbp             = r.IsDBNull(12) ? null : r.GetInt32(12),
                Dbp             = r.IsDBNull(13) ? null : r.GetInt32(13),
                Fatigue         = r.IsDBNull(14) ? null : r.GetInt32(14),
                BodyTempEvi     = r.IsDBNull(15) ? null : r.GetDouble(15),
                BodyTempEsti    = r.IsDBNull(16) ? null : r.GetInt32(16),
                TempType        = r.IsDBNull(17) ? null : r.GetInt32(17),
                BpBpm           = r.IsDBNull(18) ? null : r.GetInt32(18),
                BloodPotassium  = r.IsDBNull(19) ? null : r.GetDouble(19),
                BloodSugar      = r.IsDBNull(20) ? null : r.GetDouble(20),
                BiozR           = r.IsDBNull(21) ? null : r.GetDouble(21),
                BiozX           = r.IsDBNull(22) ? null : r.GetDouble(22),
                BiozFat         = r.IsDBNull(23) ? null : r.GetDouble(23),
                BiozBmi         = r.IsDBNull(24) ? null : r.GetDouble(24),
                BiozType        = r.IsDBNull(25) ? null : r.GetInt32(25),
                BreathRate      = r.IsDBNull(26) ? null : r.GetDouble(26),
                MoodLevel       = r.IsDBNull(27) ? null : r.GetInt32(27),
                CreatedAt       = r.GetDateTime(28)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetLatestHealthSnapshot failed for {Device}", deviceId);
            return null;
        }
    }

    public async Task<int> GetActiveWorkerCount()
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM user_profiles WHERE is_active = 1", conn);
            return (int)(await cmd.ExecuteScalarAsync())!;
        }
        catch (Exception ex) { _logger.LogError(ex, "GetActiveWorkerCount failed."); return 0; }
    }

    public async Task<IReadOnlyList<UserProfile>> GetPagedUserProfiles(int skip, int take)
    {
        var list = new List<UserProfile>();
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                SELECT u.device_id, u.user_id, u.name, u.surname, u.email, u.cell,
                       u.emp_no, u.address, u.company_id, c.name AS company_name, u.updated_at
                FROM user_profiles u
                LEFT JOIN companies c ON c.id = u.company_id
                WHERE u.is_active = 1
                ORDER BY u.surname, u.name
                OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY", conn);
            cmd.Parameters.AddWithValue("@skip", skip);
            cmd.Parameters.AddWithValue("@take", take);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                list.Add(MapUserProfile(r));
        }
        catch (Exception ex) { _logger.LogError(ex, "GetPagedUserProfiles failed."); }
        return list;
    }

    public async Task<int> GetActiveWorkerCountByCompany(int companyId)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM user_profiles WHERE is_active=1 AND company_id=@cid", conn);
            cmd.Parameters.AddWithValue("@cid", companyId);
            return (int)(await cmd.ExecuteScalarAsync())!;
        }
        catch (Exception ex) { _logger.LogError(ex, "GetActiveWorkerCountByCompany failed for {Id}", companyId); return 0; }
    }

    public async Task<IReadOnlyList<UserProfile>> GetPagedUserProfilesByCompany(int skip, int take, int companyId)
    {
        var list = new List<UserProfile>();
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                SELECT u.device_id, u.user_id, u.name, u.surname, u.email, u.cell,
                       u.emp_no, u.address, u.company_id, c.name AS company_name, u.updated_at
                FROM user_profiles u
                LEFT JOIN companies c ON c.id = u.company_id
                WHERE u.is_active = 1 AND u.company_id = @cid
                ORDER BY u.surname, u.name
                OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY", conn);
            cmd.Parameters.AddWithValue("@skip", skip);
            cmd.Parameters.AddWithValue("@take", take);
            cmd.Parameters.AddWithValue("@cid",  companyId);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                list.Add(MapUserProfile(r));
        }
        catch (Exception ex) { _logger.LogError(ex, "GetPagedUserProfilesByCompany failed for {Id}", companyId); }
        return list;
    }

    public async Task<(IReadOnlyList<UserProfileWithData> Items, int TotalCount)> GetPagedUserProfilesWithData(
        int skip, int take, int? companyId)
    {
        var list = new List<UserProfileWithData>();
        int total = 0;
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                SELECT
                    COUNT(*) OVER ()        AS total_count,
                    u.device_id, u.user_id, u.name, u.surname,
                    u.email, u.cell, u.emp_no, u.address,
                    u.company_id, c.name    AS company_name, u.updated_at,
                    h.battery, h.avg_hr, h.max_hr, h.min_hr,
                    h.avg_spo2, h.sbp, h.dbp, h.steps,
                    h.distance, h.calorie, h.fatigue, h.created_at AS health_at,
                    g.longitude, g.latitude, g.gnss_time
                FROM user_profiles u
                LEFT JOIN companies c ON c.id = u.company_id
                OUTER APPLY (
                    SELECT TOP 1
                        battery, avg_hr, max_hr, min_hr, avg_spo2,
                        sbp, dbp, steps, distance, calorie, fatigue, created_at
                    FROM health_snapshots
                    WHERE device_id = u.device_id
                    ORDER BY id DESC
                ) h
                OUTER APPLY (
                    SELECT TOP 1 longitude, latitude, gnss_time
                    FROM gps_tracks
                    WHERE device_id = u.device_id
                    ORDER BY id DESC
                ) g
                WHERE u.is_active = 1
                  AND (@cid IS NULL OR u.company_id = @cid)
                ORDER BY u.surname, u.name
                OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY", conn);
            cmd.Parameters.Add("@cid",  System.Data.SqlDbType.Int).Value = (object?)companyId ?? DBNull.Value;
            cmd.Parameters.AddWithValue("@skip", skip);
            cmd.Parameters.AddWithValue("@take", take);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                if (total == 0) total = r.GetInt32(0);
                list.Add(new UserProfileWithData
                {
                    DeviceId    = r.GetString(1),
                    UserId      = r.GetInt32(2),
                    Name        = r.GetString(3),
                    Surname     = r.GetString(4),
                    Email       = r.IsDBNull(5)  ? null : r.GetString(5),
                    Cell        = r.IsDBNull(6)  ? null : r.GetString(6),
                    EmpNo       = r.IsDBNull(7)  ? null : r.GetString(7),
                    Address     = r.IsDBNull(8)  ? null : r.GetString(8),
                    CompanyId   = r.IsDBNull(9)  ? null : r.GetInt32(9),
                    CompanyName = r.IsDBNull(10) ? null : r.GetString(10),
                    UpdatedAt   = r.GetDateTime(11),
                    Battery     = r.IsDBNull(12) ? null : r.GetInt32(12),
                    AvgHr       = r.IsDBNull(13) ? null : r.GetInt32(13),
                    MaxHr       = r.IsDBNull(14) ? null : r.GetInt32(14),
                    MinHr       = r.IsDBNull(15) ? null : r.GetInt32(15),
                    AvgSpo2     = r.IsDBNull(16) ? null : r.GetInt32(16),
                    Sbp         = r.IsDBNull(17) ? null : r.GetInt32(17),
                    Dbp         = r.IsDBNull(18) ? null : r.GetInt32(18),
                    Steps       = r.IsDBNull(19) ? null : r.GetInt32(19),
                    Distance    = r.IsDBNull(20) ? null : r.GetDouble(20),
                    Calorie     = r.IsDBNull(21) ? null : r.GetDouble(21),
                    Fatigue     = r.IsDBNull(22) ? null : r.GetInt32(22),
                    HealthAt    = r.IsDBNull(23) ? null : r.GetDateTime(23),
                    Longitude   = r.IsDBNull(24) ? null : r.GetDouble(24),
                    Latitude    = r.IsDBNull(25) ? null : r.GetDouble(25),
                    GnssTime    = r.IsDBNull(26) ? null : r.GetString(26),
                });
            }
        }
        catch (Exception ex) { _logger.LogError(ex, "GetPagedUserProfilesWithData failed"); }
        return (list, total);
    }

    public async Task<int> GetRecentAlarmCount(int withinHours)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                SELECT COUNT(*)
                FROM alarms
                WHERE created_at >= DATEADD(HOUR, -@h, GETDATE())", conn);
            cmd.Parameters.AddWithValue("@h", withinHours);
            return (int)(await cmd.ExecuteScalarAsync())!;
        }
        catch (Exception ex) { _logger.LogError(ex, "GetRecentAlarmCount failed."); return 0; }
    }

    public async Task<int> GetRecentSosCount(int withinHours)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                SELECT COUNT(*)
                FROM sos_events
                WHERE created_at >= DATEADD(HOUR, -@h, GETDATE())", conn);
            cmd.Parameters.AddWithValue("@h", withinHours);
            return (int)(await cmd.ExecuteScalarAsync())!;
        }
        catch (Exception ex) { _logger.LogError(ex, "GetRecentSosCount failed."); return 0; }
    }

    public async Task<IReadOnlyList<AlarmEvent>> GetRecentAlarms(int withinHours, int limit)
    {
        var list = new List<AlarmEvent>();
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                SELECT TOP (@limit)
                    a.id, a.device_id,
                    CASE WHEN u.name IS NOT NULL THEN u.name + ' ' + u.surname END AS worker_name,
                    a.alarm_time, a.alarm_type, a.details, a.created_at
                FROM alarms a
                LEFT JOIN user_profiles u ON u.device_id = a.device_id AND u.is_active = 1
                WHERE a.created_at >= DATEADD(HOUR, -@h, GETDATE())
                ORDER BY a.created_at DESC", conn);
            cmd.Parameters.AddWithValue("@limit", limit);
            cmd.Parameters.AddWithValue("@h",     withinHours);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new AlarmEvent
                {
                    Id         = r.GetInt32(0),
                    DeviceId   = r.GetString(1),
                    WorkerName = r.IsDBNull(2) ? null : r.GetString(2),
                    AlarmTime  = r.GetString(3),
                    AlarmType  = r.GetString(4),
                    Details    = r.IsDBNull(5) ? null : r.GetString(5),
                    CreatedAt  = r.GetDateTime(6)
                });
            }
        }
        catch (Exception ex) { _logger.LogError(ex, "GetRecentAlarms failed."); }
        return list;
    }

    public async Task<(int TotalWorkers, int SosCount, int HrAlertCount, int TrackedOnMap)> GetDashboardCounts(
        int withinHours, int? companyId = null)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                SELECT
                    (SELECT COUNT(*)
                     FROM   user_profiles
                     WHERE  is_active = 1
                       AND  (@cid IS NULL OR company_id = @cid)),
                    (SELECT COUNT(*)
                     FROM   sos_events
                     WHERE  created_at >= DATEADD(HOUR, -@h, GETDATE())
                       AND  (@cid IS NULL OR company_id = @cid)),
                    (SELECT COUNT(*)
                     FROM   alarms
                     WHERE  alarm_type = 'hr_alarm'
                       AND  created_at >= DATEADD(HOUR, -@h, GETDATE())
                       AND  (@cid IS NULL OR company_id = @cid)),
                    (SELECT COUNT(DISTINCT device_id)
                     FROM   gps_tracks
                     WHERE  created_at >= DATEADD(HOUR, -@h, GETDATE())
                       AND  (@cid IS NULL OR company_id = @cid))", conn);
            cmd.Parameters.Add("@cid", System.Data.SqlDbType.Int).Value = (object?)companyId ?? DBNull.Value;
            cmd.Parameters.AddWithValue("@h", withinHours);
            await using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync()) return (0, 0, 0, 0);
            return (r.GetInt32(0), r.GetInt32(1), r.GetInt32(2), r.GetInt32(3));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDashboardCounts failed");
            return (0, 0, 0, 0);
        }
    }

    // ── GPS queries ───────────────────────────────────────────────────────────

    public async Task<(IReadOnlyList<(string DeviceId, string? UserName, GnssTrack Track)> Items, int TotalCount)>
        GetGnssTracksByCompany(int companyId, System.DateTime? from, System.DateTime? to,
            int skip, int take, string sortDir, bool onlineOnly, bool offlineOnly)
    {
        var list  = new List<(string, string?, GnssTrack)>();
        int total = 0;
        try
        {
            var dir  = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";

            // Single query: COUNT(*) OVER() on the deduped set replaces the separate COUNT(DISTINCT) round-trip
            var sql = $@"
                WITH ranked AS (
                    SELECT g.device_id,
                           CASE WHEN u.name IS NOT NULL THEN u.name + ' ' + u.surname END AS user_name,
                           g.id, g.gnss_time, g.longitude, g.latitude, g.loc_type, g.created_at,
                           ROW_NUMBER() OVER (PARTITION BY g.device_id ORDER BY g.id DESC) AS rn
                    FROM gps_tracks g
                    INNER JOIN user_profiles u ON u.device_id = g.device_id AND u.is_active = 1
                    WHERE u.company_id = @cid
                      AND (@from IS NULL OR g.created_at >= @from)
                      AND (@to   IS NULL OR g.created_at <= @to)
                )
                SELECT device_id, user_name, id, gnss_time, longitude, latitude, loc_type, created_at,
                       COUNT(*) OVER() AS total_count
                FROM ranked
                WHERE rn = 1
                ORDER BY created_at {dir}
                OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY";

            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@cid",  companyId);
            cmd.Parameters.Add("@from", System.Data.SqlDbType.DateTime2).Value = (object?)from ?? DBNull.Value;
            cmd.Parameters.Add("@to",   System.Data.SqlDbType.DateTime2).Value = (object?)to   ?? DBNull.Value;
            cmd.Parameters.AddWithValue("@skip", skip);
            cmd.Parameters.AddWithValue("@take", take);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                if (total == 0) total = r.GetInt32(8); // total_count: COUNT(*) OVER() as column 8
                var track = new GnssTrack
                {
                    Id        = r.GetInt32(2),
                    DeviceId  = r.GetString(0),
                    GnssTime  = r.GetString(3),
                    Longitude = r.GetDouble(4),
                    Latitude  = r.GetDouble(5),
                    LocType   = r.IsDBNull(6) ? null : r.GetString(6),
                    CreatedAt = r.GetDateTime(7)
                };
                list.Add((r.GetString(0), r.IsDBNull(1) ? null : r.GetString(1), track));
            }
        }
        catch (Exception ex) { _logger.LogError(ex, "GetGnssTracksByCompany failed for company {Id}", companyId); }
        return (list, total);
    }

    public async Task<(int Online, int Offline)> GetDeviceStatusCountsByCompany(int companyId,
        System.Collections.Generic.IReadOnlyList<string> onlineDeviceIds)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(
                "SELECT device_id FROM user_profiles WHERE company_id=@cid AND is_active=1", conn);
            cmd.Parameters.AddWithValue("@cid", companyId);
            await using var r = await cmd.ExecuteReaderAsync();
            int online = 0, offline = 0;
            var onlineSet = new System.Collections.Generic.HashSet<string>(
                onlineDeviceIds, StringComparer.OrdinalIgnoreCase);
            while (await r.ReadAsync())
            {
                var did = r.GetString(0);
                if (onlineSet.Contains(did)) online++; else offline++;
            }
            return (online, offline);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDeviceStatusCountsByCompany failed for company {Id}", companyId);
            return (0, 0);
        }
    }

    // ── Health queries ────────────────────────────────────────────────────────

    public async Task<(IReadOnlyList<HealthSnapshot> Items, int TotalCount)>
        GetHealthSnapshotsByDevice(string deviceId, System.DateTime? from, System.DateTime? to,
            int skip, int take, string sortDir)
    {
        var list  = new List<HealthSnapshot>();
        int total = 0;
        try
        {
            var dir = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";
            await using var conn = await OpenAsync();

            // Single query: COUNT(*) OVER() eliminates the separate count round-trip
            using var cmd = new SqlCommand($@"
                SELECT id, device_id, record_time, battery, rssi, steps,
                       distance, calorie, avg_hr, max_hr, min_hr,
                       avg_spo2, sbp, dbp, fatigue,
                       body_temp_evi, body_temp_esti, temp_type, bp_bpm, blood_potassium, blood_sugar,
                       bioz_r, bioz_x, bioz_fat, bioz_bmi, bioz_type, breath_rate, mood_level,
                       created_at, COUNT(*) OVER() AS total_count
                FROM health_snapshots
                WHERE device_id=@dev
                  AND (@from IS NULL OR created_at >= @from)
                  AND (@to   IS NULL OR created_at <= @to)
                ORDER BY created_at {dir}
                OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY", conn);
            cmd.Parameters.AddWithValue("@dev",  deviceId);
            cmd.Parameters.Add("@from", System.Data.SqlDbType.DateTime2).Value = (object?)from ?? DBNull.Value;
            cmd.Parameters.Add("@to",   System.Data.SqlDbType.DateTime2).Value = (object?)to   ?? DBNull.Value;
            cmd.Parameters.AddWithValue("@skip", skip);
            cmd.Parameters.AddWithValue("@take", take);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                if (list.Count == 0) total = r.GetInt32(29); // total_count appended after created_at
                list.Add(MapHealthSnapshot(r));
            }
        }
        catch (Exception ex) { _logger.LogError(ex, "GetHealthSnapshotsByDevice failed for {Device}", deviceId); }
        return (list, total);
    }

    public async Task<(IReadOnlyList<(string DeviceId, string? UserName, HealthSnapshot Snapshot)> Items, int TotalCount)>
        GetHealthSnapshotsByCompany(int companyId, System.DateTime? from, System.DateTime? to,
            int skip, int take, string sortDir)
    {
        var list  = new List<(string, string?, HealthSnapshot)>();
        int total = 0;
        try
        {
            var dir = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";
            await using var conn = await OpenAsync();

            // Single query: COUNT(*) OVER() on the deduped set replaces the separate COUNT(DISTINCT) round-trip
            using var cmd = new SqlCommand($@"
                WITH ranked AS (
                    SELECT h.id, h.device_id, h.record_time, h.battery, h.rssi, h.steps,
                           h.distance, h.calorie, h.avg_hr, h.max_hr, h.min_hr,
                           h.avg_spo2, h.sbp, h.dbp, h.fatigue,
                           h.body_temp_evi, h.body_temp_esti, h.temp_type, h.bp_bpm,
                           h.blood_potassium, h.blood_sugar,
                           h.bioz_r, h.bioz_x, h.bioz_fat, h.bioz_bmi, h.bioz_type,
                           h.breath_rate, h.mood_level, h.created_at,
                           CASE WHEN u.name IS NOT NULL THEN u.name + ' ' + u.surname END AS user_name,
                           ROW_NUMBER() OVER (PARTITION BY h.device_id ORDER BY h.id DESC) AS rn
                    FROM health_snapshots h
                    INNER JOIN user_profiles u ON u.device_id=h.device_id AND u.is_active=1
                    WHERE u.company_id=@cid
                      AND (@from IS NULL OR h.created_at >= @from)
                      AND (@to   IS NULL OR h.created_at <= @to)
                )
                SELECT id, device_id, user_name, record_time, battery, rssi, steps,
                       distance, calorie, avg_hr, max_hr, min_hr,
                       avg_spo2, sbp, dbp, fatigue,
                       body_temp_evi, body_temp_esti, temp_type, bp_bpm, blood_potassium, blood_sugar,
                       bioz_r, bioz_x, bioz_fat, bioz_bmi, bioz_type, breath_rate, mood_level,
                       created_at, COUNT(*) OVER() AS total_count
                FROM ranked
                WHERE rn = 1
                ORDER BY created_at {dir}
                OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY", conn);
            cmd.Parameters.AddWithValue("@cid", companyId);
            cmd.Parameters.Add("@from", System.Data.SqlDbType.DateTime2).Value = (object?)from ?? DBNull.Value;
            cmd.Parameters.Add("@to",   System.Data.SqlDbType.DateTime2).Value = (object?)to   ?? DBNull.Value;
            cmd.Parameters.AddWithValue("@skip", skip);
            cmd.Parameters.AddWithValue("@take", take);
            // 0=id,1=device_id,2=user_name,3=record_time,4=battery,5=rssi,6=steps,
            // 7=distance,8=calorie,9=avg_hr,10=max_hr,11=min_hr,12=avg_spo2,13=sbp,14=dbp,15=fatigue,
            // 16=body_temp_evi,17=body_temp_esti,18=temp_type,19=bp_bpm,20=blood_potassium,21=blood_sugar,
            // 22=bioz_r,23=bioz_x,24=bioz_fat,25=bioz_bmi,26=bioz_type,27=breath_rate,28=mood_level,
            // 29=created_at, 30=total_count
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                if (list.Count == 0) total = r.GetInt32(30); // total_count: COUNT(*) OVER() as column 30
                var snap = new HealthSnapshot
                {
                    Id              = r.GetInt32(0),
                    DeviceId        = r.GetString(1),
                    RecordTime      = r.GetString(3),
                    Battery         = r.IsDBNull(4)  ? null : r.GetInt32(4),
                    Rssi            = r.IsDBNull(5)  ? null : r.GetInt32(5),
                    Steps           = r.IsDBNull(6)  ? null : r.GetInt32(6),
                    Distance        = r.IsDBNull(7)  ? null : r.GetDouble(7),
                    Calorie         = r.IsDBNull(8)  ? null : r.GetDouble(8),
                    AvgHr           = r.IsDBNull(9)  ? null : r.GetInt32(9),
                    MaxHr           = r.IsDBNull(10) ? null : r.GetInt32(10),
                    MinHr           = r.IsDBNull(11) ? null : r.GetInt32(11),
                    AvgSpo2         = r.IsDBNull(12) ? null : r.GetInt32(12),
                    Sbp             = r.IsDBNull(13) ? null : r.GetInt32(13),
                    Dbp             = r.IsDBNull(14) ? null : r.GetInt32(14),
                    Fatigue         = r.IsDBNull(15) ? null : r.GetInt32(15),
                    BodyTempEvi     = r.IsDBNull(16) ? null : r.GetDouble(16),
                    BodyTempEsti    = r.IsDBNull(17) ? null : r.GetInt32(17),
                    TempType        = r.IsDBNull(18) ? null : r.GetInt32(18),
                    BpBpm           = r.IsDBNull(19) ? null : r.GetInt32(19),
                    BloodPotassium  = r.IsDBNull(20) ? null : r.GetDouble(20),
                    BloodSugar      = r.IsDBNull(21) ? null : r.GetDouble(21),
                    BiozR           = r.IsDBNull(22) ? null : r.GetDouble(22),
                    BiozX           = r.IsDBNull(23) ? null : r.GetDouble(23),
                    BiozFat         = r.IsDBNull(24) ? null : r.GetDouble(24),
                    BiozBmi         = r.IsDBNull(25) ? null : r.GetDouble(25),
                    BiozType        = r.IsDBNull(26) ? null : r.GetInt32(26),
                    BreathRate      = r.IsDBNull(27) ? null : r.GetDouble(27),
                    MoodLevel       = r.IsDBNull(28) ? null : r.GetInt32(28),
                    CreatedAt       = r.GetDateTime(29)
                };
                list.Add((r.GetString(1), r.IsDBNull(2) ? null : r.GetString(2), snap));
            }
        }
        catch (Exception ex) { _logger.LogError(ex, "GetHealthSnapshotsByCompany failed for company {Id}", companyId); }
        return (list, total);
    }

    public async Task<IReadOnlyList<(string DeviceId, string? UserName, double? AvgHr, double? AvgSpo2,
        double? AvgFatigue, int? MaxHr, int? MinHr, int? TotalSteps, int Count)>>
        GetHealthSummaryByCompany(int companyId, System.DateTime? from, System.DateTime? to)
    {
        var list = new List<(string, string?, double?, double?, double?, int?, int?, int?, int)>();
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(@"
                SELECT h.device_id,
                       CASE WHEN u.name IS NOT NULL THEN u.name + ' ' + u.surname END AS user_name,
                       AVG(CAST(h.avg_hr   AS FLOAT)), AVG(CAST(h.avg_spo2 AS FLOAT)),
                       AVG(CAST(h.fatigue  AS FLOAT)),
                       MAX(h.max_hr), MIN(h.min_hr), SUM(h.steps), COUNT(*)
                FROM health_snapshots h
                INNER JOIN user_profiles u ON u.device_id=h.device_id AND u.is_active=1
                WHERE u.company_id=@cid
                  AND (@from IS NULL OR h.created_at >= @from)
                  AND (@to   IS NULL OR h.created_at <= @to)
                GROUP BY h.device_id, u.name, u.surname
                ORDER BY u.surname, u.name", conn);
            cmd.Parameters.AddWithValue("@cid", companyId);
            cmd.Parameters.Add("@from", System.Data.SqlDbType.DateTime2).Value = (object?)from ?? DBNull.Value;
            cmd.Parameters.Add("@to",   System.Data.SqlDbType.DateTime2).Value = (object?)to   ?? DBNull.Value;
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add((
                    r.GetString(0),
                    r.IsDBNull(1) ? null : r.GetString(1),
                    r.IsDBNull(2) ? null : r.GetDouble(2),
                    r.IsDBNull(3) ? null : r.GetDouble(3),
                    r.IsDBNull(4) ? null : r.GetDouble(4),
                    r.IsDBNull(5) ? null : r.GetInt32(5),
                    r.IsDBNull(6) ? null : r.GetInt32(6),
                    r.IsDBNull(7) ? null : (int?)Math.Min(r.GetInt64(7), int.MaxValue),
                    r.GetInt32(8)
                ));
            }
        }
        catch (Exception ex) { _logger.LogError(ex, "GetHealthSummaryByCompany failed for company {Id}", companyId); }
        return list;
    }

    // ── Device configuration queries ──────────────────────────────────────────

    private static readonly string DeviceConfigSelectCols = @"
        u.device_id,
        CASE WHEN u.name IS NOT NULL THEN u.name + ' ' + u.surname END AS user_name,
        GREATEST(df.updated_at, lf.updated_at, ha.updated_at, dha.updated_at,
                 sa.updated_at, ba.updated_at, ta.updated_at, fs.updated_at,
                 dd.updated_at, hi.updated_at, oi.updated_at, dg.updated_at,
                 gs.updated_at, lg.updated_at, aaf.updated_at, bpa.updated_at) AS last_updated,
        df.gps_auto_check, df.gps_interval_time, df.power_mode,
        lf.data_auto_upload, lf.data_upload_interval, lf.auto_locate, lf.locate_interval_time,
        ha.[open]  AS hr_alarm_open,  ha.high AS hr_alarm_high, ha.low AS hr_alarm_low,
        ha.threshold, ha.alarm_interval,
        dha.[open] AS dyn_hr_open, dha.high AS dyn_hr_high, dha.low AS dyn_hr_low,
        dha.timeout, dha.interval AS dyn_hr_interval,
        sa.[open]  AS spo2_open,  sa.low  AS spo2_low,
        ba.[open]  AS bp_open,    ba.sbp_high, ba.sbp_below, ba.dbp_high, ba.dbp_below,
        ta.[open]  AS temp_open,  ta.high AS temp_high, ta.low AS temp_low,
        fs.fall_check, fs.fall_threshold,
        dd.language, dd.hour_format, dd.date_format, dd.distance_unit, dd.temperature_unit, dd.wear_hand_right,
        hi.interval AS hr_interval,
        oi.interval AS other_interval,
        dg.step, dg.distance AS goal_distance, dg.calorie AS goal_calorie,
        gs.gps_auto_check AS gps_locate_auto, gs.gps_interval_time AS gps_locate_interval, gs.run_gps,
        lg.[open] AS lcd_open, lg.start_hour AS lcd_start, lg.end_hour AS lcd_end,
        aaf.[open] AS af_open, aaf.interval AS af_interval,
        bpa.sbp_band, bpa.dbp_band, bpa.sbp_meter, bpa.dbp_meter";

    private static readonly string DeviceConfigJoins = @"
        LEFT JOIN device_data_freq      df  ON df.device_id  = u.device_id
        LEFT JOIN device_locate_freq    lf  ON lf.device_id  = u.device_id
        LEFT JOIN device_hr_alarm       ha  ON ha.device_id  = u.device_id
        LEFT JOIN device_dynamic_hr_alarm dha ON dha.device_id = u.device_id
        LEFT JOIN device_spo2_alarm     sa  ON sa.device_id  = u.device_id
        LEFT JOIN device_bp_alarm       ba  ON ba.device_id  = u.device_id
        LEFT JOIN device_temp_alarm     ta  ON ta.device_id  = u.device_id
        LEFT JOIN device_fall_settings  fs  ON fs.device_id  = u.device_id
        LEFT JOIN device_display        dd  ON dd.device_id  = u.device_id
        LEFT JOIN device_hr_interval    hi  ON hi.device_id  = u.device_id
        LEFT JOIN device_other_interval oi  ON oi.device_id  = u.device_id
        LEFT JOIN device_goal           dg  ON dg.device_id  = u.device_id
        LEFT JOIN device_gps_settings   gs  ON gs.device_id  = u.device_id
        LEFT JOIN device_lcd_gesture    lg  ON lg.device_id  = u.device_id
        LEFT JOIN device_auto_af        aaf ON aaf.device_id = u.device_id
        LEFT JOIN device_bp_adjust      bpa ON bpa.device_id = u.device_id";

    private static (string DeviceId, string? UserName, System.DateTime? UpdatedAt,
        bool? GpsAutoCheck, int? GpsIntervalTime, int? PowerMode,
        bool? DataAutoUpload, int? DataUploadInterval, bool? AutoLocate, int? LocateIntervalTime,
        bool? HrAlarmOpen, int? HrAlarmHigh, int? HrAlarmLow, int? HrAlarmThreshold, int? HrAlarmInterval,
        bool? DynHrAlarmOpen, int? DynHrAlarmHigh, int? DynHrAlarmLow, int? DynHrAlarmTimeout, int? DynHrAlarmInterval,
        bool? Spo2AlarmOpen, int? Spo2AlarmLow,
        bool? BpAlarmOpen, int? BpSbpHigh, int? BpSbpBelow, int? BpDbpHigh, int? BpDbpBelow,
        bool? TempAlarmOpen, double? TempAlarmHigh, double? TempAlarmLow,
        bool? FallCheckEnabled, int? FallThreshold,
        string? Language, int? HourFormat, string? DateFormat, int? DistanceUnit, int? TemperatureUnit, bool? WearHandRight,
        int? HrInterval, int? OtherInterval,
        int? GoalStep, double? GoalDistance, double? GoalCalorie,
        bool? GpsLocateAutoCheck, int? GpsLocateIntervalTime, bool? RunGps,
        bool? LcdGestureOpen, int? LcdGestureStartHour, int? LcdGestureEndHour,
        bool? AutoAfOpen, int? AutoAfInterval,
        double? BpSbpBand, double? BpDbpBand, double? BpSbpMeter, double? BpDbpMeter)
        MapDeviceConfigRow(SqlDataReader r)
    {
        bool? B(int i) => r.IsDBNull(i) ? null : r.GetBoolean(i);
        int?  I(int i) => r.IsDBNull(i) ? null : r.GetInt32(i);
        double? D(int i) => r.IsDBNull(i) ? null : r.GetDouble(i);
        string? S(int i) => r.IsDBNull(i) ? null : r.GetString(i);

        return (
            r.GetString(0), S(1), r.IsDBNull(2) ? null : r.GetDateTime(2),
            B(3),  I(4),  I(5),
            B(6),  I(7),  B(8),  I(9),
            B(10), I(11), I(12), I(13), I(14),
            B(15), I(16), I(17), I(18), I(19),
            B(20), I(21),
            B(22), I(23), I(24), I(25), I(26),
            B(27), D(28), D(29),
            B(30), I(31),
            S(32), I(33), S(34), I(35), I(36), B(37),
            I(38), I(39),
            I(40), D(41), D(42),
            B(43), I(44), B(45),
            B(46), I(47), I(48),
            B(49), I(50),
            D(51), D(52), D(53), D(54)
        );
    }

    public async Task<(string DeviceId, string? UserName, System.DateTime? UpdatedAt,
        bool? GpsAutoCheck, int? GpsIntervalTime, int? PowerMode,
        bool? DataAutoUpload, int? DataUploadInterval, bool? AutoLocate, int? LocateIntervalTime,
        bool? HrAlarmOpen, int? HrAlarmHigh, int? HrAlarmLow, int? HrAlarmThreshold, int? HrAlarmInterval,
        bool? DynHrAlarmOpen, int? DynHrAlarmHigh, int? DynHrAlarmLow, int? DynHrAlarmTimeout, int? DynHrAlarmInterval,
        bool? Spo2AlarmOpen, int? Spo2AlarmLow,
        bool? BpAlarmOpen, int? BpSbpHigh, int? BpSbpBelow, int? BpDbpHigh, int? BpDbpBelow,
        bool? TempAlarmOpen, double? TempAlarmHigh, double? TempAlarmLow,
        bool? FallCheckEnabled, int? FallThreshold,
        string? Language, int? HourFormat, string? DateFormat, int? DistanceUnit, int? TemperatureUnit, bool? WearHandRight,
        int? HrInterval, int? OtherInterval,
        int? GoalStep, double? GoalDistance, double? GoalCalorie,
        bool? GpsLocateAutoCheck, int? GpsLocateIntervalTime, bool? RunGps,
        bool? LcdGestureOpen, int? LcdGestureStartHour, int? LcdGestureEndHour,
        bool? AutoAfOpen, int? AutoAfInterval,
        double? BpSbpBand, double? BpDbpBand, double? BpSbpMeter, double? BpDbpMeter)?>
        GetDeviceConfig(string deviceId)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand($@"
                SELECT {DeviceConfigSelectCols}
                FROM user_profiles u
                {DeviceConfigJoins}
                WHERE u.device_id = @dev AND u.is_active = 1", conn);
            cmd.Parameters.AddWithValue("@dev", deviceId);
            await using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync()) return null;
            return MapDeviceConfigRow(r);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDeviceConfig failed for {Device}", deviceId);
            return null;
        }
    }

    public async Task<IReadOnlyList<(string DeviceId, string? UserName, System.DateTime? UpdatedAt,
        bool? GpsAutoCheck, int? GpsIntervalTime, int? PowerMode,
        bool? DataAutoUpload, int? DataUploadInterval, bool? AutoLocate, int? LocateIntervalTime,
        bool? HrAlarmOpen, int? HrAlarmHigh, int? HrAlarmLow, int? HrAlarmThreshold, int? HrAlarmInterval,
        bool? DynHrAlarmOpen, int? DynHrAlarmHigh, int? DynHrAlarmLow, int? DynHrAlarmTimeout, int? DynHrAlarmInterval,
        bool? Spo2AlarmOpen, int? Spo2AlarmLow,
        bool? BpAlarmOpen, int? BpSbpHigh, int? BpSbpBelow, int? BpDbpHigh, int? BpDbpBelow,
        bool? TempAlarmOpen, double? TempAlarmHigh, double? TempAlarmLow,
        bool? FallCheckEnabled, int? FallThreshold,
        string? Language, int? HourFormat, string? DateFormat, int? DistanceUnit, int? TemperatureUnit, bool? WearHandRight,
        int? HrInterval, int? OtherInterval,
        int? GoalStep, double? GoalDistance, double? GoalCalorie,
        bool? GpsLocateAutoCheck, int? GpsLocateIntervalTime, bool? RunGps,
        bool? LcdGestureOpen, int? LcdGestureStartHour, int? LcdGestureEndHour,
        bool? AutoAfOpen, int? AutoAfInterval,
        double? BpSbpBand, double? BpDbpBand, double? BpSbpMeter, double? BpDbpMeter)>>
        GetDeviceConfigsByCompany(int companyId, int skip, int take)
    {
        var list = new List<(string, string?, System.DateTime?,
            bool?, int?, int?, bool?, int?, bool?, int?,
            bool?, int?, int?, int?, int?,
            bool?, int?, int?, int?, int?,
            bool?, int?,
            bool?, int?, int?, int?, int?,
            bool?, double?, double?,
            bool?, int?,
            string?, int?, string?, int?, int?, bool?,
            int?, int?,
            int?, double?, double?,
            bool?, int?, bool?,
            bool?, int?, int?,
            bool?, int?,
            double?, double?, double?, double?)>();
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand($@"
                SELECT {DeviceConfigSelectCols}
                FROM user_profiles u
                {DeviceConfigJoins}
                WHERE u.company_id = @cid AND u.is_active = 1
                ORDER BY u.surname, u.name
                OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY", conn);
            cmd.Parameters.AddWithValue("@cid",  companyId);
            cmd.Parameters.AddWithValue("@skip", skip);
            cmd.Parameters.AddWithValue("@take", take);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(MapDeviceConfigRow(r));
        }
        catch (Exception ex) { _logger.LogError(ex, "GetDeviceConfigsByCompany failed for company {Id}", companyId); }
        return list;
    }

    public async Task<int> GetDeviceConfigCountByCompany(int companyId)
    {
        try
        {
            await using var conn = await OpenAsync();
            using var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM user_profiles WHERE company_id=@cid AND is_active=1", conn);
            cmd.Parameters.AddWithValue("@cid", companyId);
            return (int)(await cmd.ExecuteScalarAsync())!;
        }
        catch (Exception ex) { _logger.LogError(ex, "GetDeviceConfigCountByCompany failed for company {Id}", companyId); return 0; }
    }

    private static HealthSnapshot MapHealthSnapshot(SqlDataReader r) => new()
    {
        Id              = r.GetInt32(0),
        DeviceId        = r.GetString(1),
        RecordTime      = r.GetString(2),
        Battery         = r.IsDBNull(3)  ? null : r.GetInt32(3),
        Rssi            = r.IsDBNull(4)  ? null : r.GetInt32(4),
        Steps           = r.IsDBNull(5)  ? null : r.GetInt32(5),
        Distance        = r.IsDBNull(6)  ? null : r.GetDouble(6),
        Calorie         = r.IsDBNull(7)  ? null : r.GetDouble(7),
        AvgHr           = r.IsDBNull(8)  ? null : r.GetInt32(8),
        MaxHr           = r.IsDBNull(9)  ? null : r.GetInt32(9),
        MinHr           = r.IsDBNull(10) ? null : r.GetInt32(10),
        AvgSpo2         = r.IsDBNull(11) ? null : r.GetInt32(11),
        Sbp             = r.IsDBNull(12) ? null : r.GetInt32(12),
        Dbp             = r.IsDBNull(13) ? null : r.GetInt32(13),
        Fatigue         = r.IsDBNull(14) ? null : r.GetInt32(14),
        BodyTempEvi     = r.IsDBNull(15) ? null : r.GetDouble(15),
        BodyTempEsti    = r.IsDBNull(16) ? null : r.GetInt32(16),
        TempType        = r.IsDBNull(17) ? null : r.GetInt32(17),
        BpBpm           = r.IsDBNull(18) ? null : r.GetInt32(18),
        BloodPotassium  = r.IsDBNull(19) ? null : r.GetDouble(19),
        BloodSugar      = r.IsDBNull(20) ? null : r.GetDouble(20),
        BiozR           = r.IsDBNull(21) ? null : r.GetDouble(21),
        BiozX           = r.IsDBNull(22) ? null : r.GetDouble(22),
        BiozFat         = r.IsDBNull(23) ? null : r.GetDouble(23),
        BiozBmi         = r.IsDBNull(24) ? null : r.GetDouble(24),
        BiozType        = r.IsDBNull(25) ? null : r.GetInt32(25),
        BreathRate      = r.IsDBNull(26) ? null : r.GetDouble(26),
        MoodLevel       = r.IsDBNull(27) ? null : r.GetInt32(27),
        CreatedAt       = r.GetDateTime(28)
    };

    // ── Audit helpers ─────────────────────────────────────────────────────────

    private async Task LogAuditAsync(SqlConnection conn, string action, string tableName,
        string? deviceId = null, string? details = null)
    {
        try
        {
            using var cmd = new SqlCommand(
                "INSERT INTO audit_log (action, table_name, device_id, details) VALUES (@a, @t, @d, @det)", conn);
            cmd.Parameters.AddWithValue("@a",   action);
            cmd.Parameters.AddWithValue("@t",   tableName);
            cmd.Parameters.AddWithValue("@d",   (object?)deviceId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@det", (object?)details  ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Audit log write failed for {Action}/{Table}", action, tableName);
        }
    }

    public async Task<(IReadOnlyList<AuditEntry> Items, int TotalCount)> GetAuditLog(
        string? deviceId = null,
        string? action = null,
        string? tableName = null,
        System.DateTime? from = null,
        System.DateTime? to = null,
        int skip = 0,
        int take = 50)
    {
        var items = new List<AuditEntry>();
        int total = 0;
        try
        {
            await using var conn = await OpenAsync();

            const string where = @"
                WHERE (@dev    IS NULL OR device_id  = @dev)
                  AND (@action IS NULL OR action     = @action)
                  AND (@table  IS NULL OR table_name = @table)
                  AND (@from   IS NULL OR occurred_at >= @from)
                  AND (@to     IS NULL OR occurred_at <= @to)";

            using (var countCmd = new SqlCommand("SELECT COUNT(*) FROM audit_log" + where, conn))
            {
                countCmd.Parameters.AddWithValue("@dev",    (object?)deviceId  ?? DBNull.Value);
                countCmd.Parameters.AddWithValue("@action", (object?)action    ?? DBNull.Value);
                countCmd.Parameters.AddWithValue("@table",  (object?)tableName ?? DBNull.Value);
                countCmd.Parameters.Add("@from", System.Data.SqlDbType.DateTime2).Value = (object?)from ?? DBNull.Value;
                countCmd.Parameters.Add("@to",   System.Data.SqlDbType.DateTime2).Value = (object?)to   ?? DBNull.Value;
                total = (int)(await countCmd.ExecuteScalarAsync())!;
            }

            using var cmd = new SqlCommand(@"
                SELECT id, action, table_name, device_id, details, occurred_at
                FROM audit_log" + where + @"
                ORDER BY occurred_at DESC
                OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY", conn);
            cmd.Parameters.AddWithValue("@dev",    (object?)deviceId  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@action", (object?)action    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@table",  (object?)tableName ?? DBNull.Value);
            cmd.Parameters.Add("@from", System.Data.SqlDbType.DateTime2).Value = (object?)from ?? DBNull.Value;
            cmd.Parameters.Add("@to",   System.Data.SqlDbType.DateTime2).Value = (object?)to   ?? DBNull.Value;
            cmd.Parameters.AddWithValue("@skip", skip);
            cmd.Parameters.AddWithValue("@take", take);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                items.Add(new AuditEntry
                {
                    Id         = reader.GetInt64(0),
                    Action     = reader.GetString(1),
                    TableName  = reader.GetString(2),
                    DeviceId   = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Details    = reader.IsDBNull(4) ? null : reader.GetString(4),
                    OccurredAt = reader.GetDateTime(5)
                });
        }
        catch (Exception ex) { _logger.LogError(ex, "GetAuditLog failed."); }
        return (items, total);
    }

    public void Dispose() { }
}
