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

    private string MasterConnStr()
    {
        var b = new SqlConnectionStringBuilder(_connStr) { InitialCatalog = "master" };
        // Prefix with "tcp:" to force TCP/IP and avoid Named Pipes failures under IIS.
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
        EnsureDatabase();

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
    open       BIT          NULL,
    start_hour INT          NULL,
    end_hour   INT          NULL,
    user_id    INT          NULL,
    company_id INT          NULL,
    updated_at DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_hr_alarm')
CREATE TABLE device_hr_alarm (
    device_id     NVARCHAR(50) PRIMARY KEY,
    open          BIT          NULL,
    high          INT          NULL,
    low           INT          NULL,
    threshold     INT          NULL,
    alarm_interval INT         NULL,
    user_id        INT         NULL,
    company_id     INT         NULL,
    updated_at    DATETIME2    DEFAULT GETDATE()
);

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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_spo2_alarm')
CREATE TABLE device_spo2_alarm (
    device_id  NVARCHAR(50) PRIMARY KEY,
    open       BIT          NULL,
    low        INT          NULL,
    user_id    INT          NULL,
    company_id INT          NULL,
    updated_at DATETIME2    DEFAULT GETDATE()
);

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
    id           INT IDENTITY(1,1) PRIMARY KEY,
    device_id    NVARCHAR(50)  NOT NULL,
    record_date  NVARCHAR(10)  NOT NULL,
    completed    INT           NOT NULL,
    start_time   NVARCHAR(30)  NULL,
    end_time     NVARCHAR(30)  NULL,
    hr           INT           NULL,
    turn_times   INT           NULL,
    resp_avg     FLOAT         NULL,
    resp_max     FLOAT         NULL,
    resp_min     FLOAT         NULL,
    sections     NVARCHAR(MAX) NULL,
    user_id      INT           NULL,
    company_id   INT           NULL,
    created_at   DATETIME2     DEFAULT GETDATE()
);

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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "InitializeSchema failed.");
        }
    }

    public void InsertGpsTrack(string deviceId, string gnssTime, double longitude, double latitude, string locType)
    {
        try
        {
            using var conn = Open(); conn.Open();
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
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { _logger.LogError(ex, "InsertGpsTrack failed for {Device}", deviceId); }
    }

    public void UpsertHealthSnapshot(string deviceId, string recordTime,
        int? battery = null, int? rssi = null,
        int? steps = null, double? distance = null, double? calorie = null,
        int? avgHr = null, int? maxHr = null, int? minHr = null,
        int? avgSpo2 = null, int? sbp = null, int? dbp = null, int? fatigue = null)
    {
        try
        {
            using var conn = Open(); conn.Open();
            using var cmd = new SqlCommand(@"
                MERGE health_snapshots AS t
                USING (
                    SELECT @dev AS device_id, @rt AS record_time, u.user_id, u.company_id
                    FROM (VALUES(1)) AS x(dummy)
                    LEFT JOIN user_profiles u ON u.device_id=@dev AND u.is_active=1
                ) AS s ON t.device_id = s.device_id AND t.record_time = s.record_time
                WHEN MATCHED THEN UPDATE SET
                    battery  = COALESCE(@bat,  t.battery),
                    rssi     = COALESCE(@rssi, t.rssi),
                    steps    = COALESCE(@stp,  t.steps),
                    distance = COALESCE(@dist, t.distance),
                    calorie  = COALESCE(@cal,  t.calorie),
                    avg_hr   = COALESCE(@ahr,  t.avg_hr),
                    max_hr   = COALESCE(@xhr,  t.max_hr),
                    min_hr   = COALESCE(@nhr,  t.min_hr),
                    avg_spo2 = COALESCE(@spo,  t.avg_spo2),
                    sbp      = COALESCE(@sbp,  t.sbp),
                    dbp      = COALESCE(@dbp,  t.dbp),
                    fatigue  = COALESCE(@fat,  t.fatigue),
                    user_id    = COALESCE(s.user_id,    t.user_id),
                    company_id = COALESCE(s.company_id, t.company_id)
                WHEN NOT MATCHED THEN INSERT
                    (device_id, record_time, battery, rssi, steps, distance, calorie, avg_hr, max_hr, min_hr, avg_spo2, sbp, dbp, fatigue, user_id, company_id)
                    VALUES (s.device_id, s.record_time, @bat, @rssi, @stp, @dist, @cal, @ahr, @xhr, @nhr, @spo, @sbp, @dbp, @fat, s.user_id, s.company_id);", conn);

            cmd.Parameters.AddWithValue("@dev",  deviceId);
            cmd.Parameters.AddWithValue("@rt",   recordTime);
            cmd.Parameters.AddWithValue("@bat",  (object?)battery  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@rssi", (object?)rssi     ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@stp",  (object?)steps    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dist", (object?)distance ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cal",  (object?)calorie  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ahr",  (object?)avgHr    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@xhr",  (object?)maxHr    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@nhr",  (object?)minHr    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@spo",  (object?)avgSpo2  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@sbp",  (object?)sbp      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dbp",  (object?)dbp      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@fat",  (object?)fatigue  ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { _logger.LogError(ex, "UpsertHealthSnapshot failed for {Device}", deviceId); }
    }

    public void InsertAlarm(string deviceId, string alarmTime, string alarmType, string? details = null)
    {
        try
        {
            using var conn = Open(); conn.Open();
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
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { _logger.LogError(ex, "InsertAlarm failed for {Device}", deviceId); }
    }

    public void InsertSosEvent(string deviceId, string alarmTime,
        double? lat, double? lon,
        string? callNumber, int? callStatus, string? callStart, string? callEnd)
    {
        try
        {
            using var conn = Open(); conn.Open();
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
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { _logger.LogError(ex, "InsertSosEvent failed for {Device}", deviceId); }
    }

    public void InsertDeviceInfo(string deviceId, string recordedAt,
        string? model, string? version, string? wearingStatus, string? signal, string rawJson)
    {
        try
        {
            using var conn = Open(); conn.Open();
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
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { _logger.LogError(ex, "InsertDeviceInfo failed for {Device}", deviceId); }
    }

    public void InsertSleepCalculation(string deviceId, string recordDate,
        int completed, string? startTime, string? endTime, int hr, int turnTimes,
        double? respAvg, double? respMax, double? respMin, string? sectionsJson)
    {
        try
        {
            using var conn = Open(); conn.Open();
            using var cmd = new SqlCommand(@"
                INSERT INTO sleep_calculations
                    (device_id,record_date,completed,start_time,end_time,hr,turn_times,resp_avg,resp_max,resp_min,sections,user_id,company_id)
                SELECT @dev,@rd,@comp,@st,@et,@hr,@tt,@ra,@rx,@rn,@sec, u.user_id, u.company_id
                FROM (VALUES(1)) AS x(dummy)
                LEFT JOIN user_profiles u ON u.device_id=@dev AND u.is_active=1", conn);
            cmd.Parameters.AddWithValue("@dev",  deviceId);
            cmd.Parameters.AddWithValue("@rd",   recordDate);
            cmd.Parameters.AddWithValue("@comp", completed);
            cmd.Parameters.AddWithValue("@st",   (object?)startTime  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@et",   (object?)endTime    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@hr",   hr);
            cmd.Parameters.AddWithValue("@tt",   turnTimes);
            cmd.Parameters.AddWithValue("@ra",   (object?)respAvg    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@rx",   (object?)respMax    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@rn",   (object?)respMin    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@sec",  (object?)sectionsJson ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { _logger.LogError(ex, "InsertSleepCalculation failed for {Device}", deviceId); }
    }

    public void InsertEcgCalculation(string deviceId, int result, int hr, int effective, int direction)
    {
        try
        {
            using var conn = Open(); conn.Open();
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
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { _logger.LogError(ex, "InsertEcgCalculation failed for {Device}", deviceId); }
    }

    public void InsertAfCalculation(string deviceId, int result)
    {
        try
        {
            using var conn = Open(); conn.Open();
            using var cmd = new SqlCommand(@"
                INSERT INTO af_calculations (device_id,result,user_id,company_id)
                SELECT @dev,@res, u.user_id, u.company_id
                FROM (VALUES(1)) AS x(dummy)
                LEFT JOIN user_profiles u ON u.device_id=@dev AND u.is_active=1", conn);
            cmd.Parameters.AddWithValue("@dev", deviceId);
            cmd.Parameters.AddWithValue("@res", result);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { _logger.LogError(ex, "InsertAfCalculation failed for {Device}", deviceId); }
    }

    public void InsertSpo2Calculation(string deviceId, double spo2Score, int? oshahsRisk)
    {
        try
        {
            using var conn = Open(); conn.Open();
            using var cmd = new SqlCommand(@"
                INSERT INTO spo2_calculations (device_id,spo2_score,osahs_risk,user_id,company_id)
                SELECT @dev,@score,@risk, u.user_id, u.company_id
                FROM (VALUES(1)) AS x(dummy)
                LEFT JOIN user_profiles u ON u.device_id=@dev AND u.is_active=1", conn);
            cmd.Parameters.AddWithValue("@dev",   deviceId);
            cmd.Parameters.AddWithValue("@score", spo2Score);
            cmd.Parameters.AddWithValue("@risk",  (object?)oshahsRisk ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { _logger.LogError(ex, "InsertSpo2Calculation failed for {Device}", deviceId); }
    }

    public void UpsertUserProfile(string deviceId, string name, string surname,
        string? email = null, string? cell = null, string? empNo = null, string? address = null,
        int? companyId = null)
    {
        try
        {
            using var conn = Open(); conn.Open();
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
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { _logger.LogError(ex, "UpsertUserProfile failed for {Device}", deviceId); }
    }

    public UserProfile? GetUserProfile(string deviceId)
    {
        try
        {
            using var conn = Open(); conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT u.device_id, u.user_id, u.name, u.surname, u.email, u.cell,
                       u.emp_no, u.address, u.company_id, c.name AS company_name, u.updated_at
                FROM user_profiles u
                LEFT JOIN companies c ON c.id = u.company_id
                WHERE u.device_id = @dev AND u.is_active = 1", conn);
            cmd.Parameters.AddWithValue("@dev", deviceId);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return MapUserProfile(reader);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserProfile failed for {Device}", deviceId);
            return null;
        }
    }

    public IReadOnlyList<UserProfile> GetAllUserProfiles()
    {
        var list = new List<UserProfile>();
        try
        {
            using var conn = Open(); conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT u.device_id, u.user_id, u.name, u.surname, u.email, u.cell,
                       u.emp_no, u.address, u.company_id, c.name AS company_name, u.updated_at
                FROM user_profiles u
                LEFT JOIN companies c ON c.id = u.company_id
                WHERE u.is_active = 1
                ORDER BY u.surname, u.name", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(MapUserProfile(reader));
        }
        catch (Exception ex) { _logger.LogError(ex, "GetAllUserProfiles failed."); }
        return list;
    }

    public IReadOnlyList<UserProfile> GetUsersByCompanyId(int companyId)
    {
        var list = new List<UserProfile>();
        try
        {
            using var conn = Open(); conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT u.device_id, u.user_id, u.name, u.surname, u.email, u.cell,
                       u.emp_no, u.address, u.company_id, c.name AS company_name, u.updated_at
                FROM user_profiles u
                LEFT JOIN companies c ON c.id = u.company_id
                WHERE u.is_active = 1 AND u.company_id = @companyId
                ORDER BY u.surname, u.name", conn);
            cmd.Parameters.AddWithValue("@companyId", companyId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(MapUserProfile(reader));
        }
        catch (Exception ex) { _logger.LogError(ex, "GetUsersByCompanyId failed for company {Id}", companyId); }
        return list;
    }

    public void ReactivateUserProfile(string deviceId)
    {
        try
        {
            using var conn = Open(); conn.Open();
            using var cmd = new SqlCommand(@"
                UPDATE user_profiles
                SET is_active = 1, updated_at = GETDATE()
                WHERE device_id = @dev AND is_active = 0", conn);
            cmd.Parameters.AddWithValue("@dev", deviceId);
            cmd.ExecuteNonQuery();
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

    public void DeleteUserProfile(string deviceId)
    {
        try
        {
            using var conn = Open(); conn.Open();
            using var cmd = new SqlCommand(@"
                UPDATE user_profiles
                SET is_active = 0, updated_at = GETDATE()
                WHERE device_id = @dev AND is_active = 1", conn);
            cmd.Parameters.AddWithValue("@dev", deviceId);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { _logger.LogError(ex, "DeleteUserProfile failed for {Device}", deviceId); }
    }

    // ── Company CRUD ───────────────────────────────────────────────────────────

    public int CreateCompany(string name, string? registrationNumber, string? contactEmail,
        string? contactPhone, string? address)
    {
        try
        {
            using var conn = Open(); conn.Open();
            using var cmd = new SqlCommand(@"
                INSERT INTO companies (name, registration_number, contact_email, contact_phone, address)
                OUTPUT INSERTED.id
                VALUES (@name, @reg, @email, @phone, @addr)", conn);
            cmd.Parameters.AddWithValue("@name",  name);
            cmd.Parameters.AddWithValue("@reg",   (object?)registrationNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@email", (object?)contactEmail       ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@phone", (object?)contactPhone       ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@addr",  (object?)address            ?? DBNull.Value);
            return (int)cmd.ExecuteScalar()!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateCompany failed for '{Name}'", name);
            return -1;
        }
    }

    public Company? GetCompany(int id)
    {
        try
        {
            using var conn = Open(); conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT id, name, registration_number, contact_email, contact_phone,
                       address, is_active, created_at, updated_at
                FROM companies
                WHERE id = @id AND is_active = 1", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return MapCompany(reader);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCompany failed for id={Id}", id);
            return null;
        }
    }

    public IReadOnlyList<Company> GetAllCompanies()
    {
        var list = new List<Company>();
        try
        {
            using var conn = Open(); conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT id, name, registration_number, contact_email, contact_phone,
                       address, is_active, created_at, updated_at
                FROM companies
                WHERE is_active = 1
                ORDER BY name", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
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

    public void UpdateCompany(int id, string name, string? registrationNumber, string? contactEmail,
        string? contactPhone, string? address)
    {
        try
        {
            using var conn = Open(); conn.Open();
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
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { _logger.LogError(ex, "UpdateCompany failed for id={Id}", id); }
    }

    public void DeleteCompany(int id)
    {
        try
        {
            using var conn = Open(); conn.Open();
            using var cmd = new SqlCommand(@"
                UPDATE companies SET is_active = 0, updated_at = GETDATE() WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { _logger.LogError(ex, "DeleteCompany failed for id={Id}", id); }
    }

    public void LinkUserToCompany(string deviceId, int? companyId)
    {
        try
        {
            using var conn = Open(); conn.Open();
            using var cmd = new SqlCommand(@"
                UPDATE user_profiles
                SET company_id = @companyId, updated_at = GETDATE()
                WHERE device_id = @dev AND is_active = 1", conn);
            cmd.Parameters.AddWithValue("@dev",       deviceId);
            cmd.Parameters.AddWithValue("@companyId", (object?)companyId ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { _logger.LogError(ex, "LinkUserToCompany failed for {Device}", deviceId); }
    }

    public int BackfillDeviceRecords(string deviceId)
    {
        try
        {
            using var conn = Open(); conn.Open();

            int? userId = null, companyId = null;
            using (var uCmd = new SqlCommand(
                "SELECT user_id, company_id FROM user_profiles WHERE device_id=@dev AND is_active=1", conn))
            {
                uCmd.Parameters.AddWithValue("@dev", deviceId);
                using var rdr = uCmd.ExecuteReader();
                if (rdr.Read())
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
                "sleep_calculations", "ecg_calculations", "af_calculations", "spo2_calculations"
            ];

            int total = 0;
            foreach (var tbl in tables)
            {
                using var upd = new SqlCommand(
                    $"UPDATE [{tbl}] SET user_id=@uid, company_id=@cid WHERE device_id=@dev", conn);
                upd.Parameters.AddWithValue("@dev", deviceId);
                upd.Parameters.AddWithValue("@uid", (object?)userId    ?? DBNull.Value);
                upd.Parameters.AddWithValue("@cid", (object?)companyId ?? DBNull.Value);
                total += upd.ExecuteNonQuery();
            }
            return total;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BackfillDeviceRecords failed for {Device}", deviceId);
            return -1;
        }
    }

    public GnssTrack? GetLatestGnssTrack(string deviceId)
    {
        try
        {
            using var conn = Open(); conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT TOP 1 id, device_id, gnss_time, longitude, latitude, loc_type, created_at
                FROM gps_tracks
                WHERE device_id = @dev
                ORDER BY id DESC", conn);
            cmd.Parameters.AddWithValue("@dev", deviceId);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
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

    public IReadOnlyList<GnssTrack> GetGnssTracks(string deviceId, System.DateTime? from, System.DateTime? to)
    {
        var list = new List<GnssTrack>();
        try
        {
            using var conn = Open(); conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT id, device_id, gnss_time, longitude, latitude, loc_type, created_at
                FROM gps_tracks
                WHERE device_id = @dev
                  AND (@from IS NULL OR created_at >= @from)
                  AND (@to   IS NULL OR created_at <= @to)
                ORDER BY created_at ASC, id ASC", conn);
            cmd.Parameters.AddWithValue("@dev",  deviceId);
            cmd.Parameters.Add("@from", System.Data.SqlDbType.DateTime2).Value = (object?)from ?? DBNull.Value;
            cmd.Parameters.Add("@to",   System.Data.SqlDbType.DateTime2).Value = (object?)to   ?? DBNull.Value;
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
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

    public HealthSnapshot? GetLatestHealthSnapshot(string deviceId)
    {
        try
        {
            using var conn = Open(); conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT TOP 1 id, device_id, record_time, battery, rssi, steps,
                       distance, calorie, avg_hr, max_hr, min_hr,
                       avg_spo2, sbp, dbp, fatigue, created_at
                FROM health_snapshots
                WHERE device_id = @dev
                ORDER BY id DESC", conn);
            cmd.Parameters.AddWithValue("@dev", deviceId);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;
            return new HealthSnapshot
            {
                Id         = r.GetInt32(0),
                DeviceId   = r.GetString(1),
                RecordTime = r.GetString(2),
                Battery    = r.IsDBNull(3)  ? null : r.GetInt32(3),
                Rssi       = r.IsDBNull(4)  ? null : r.GetInt32(4),
                Steps      = r.IsDBNull(5)  ? null : r.GetInt32(5),
                Distance   = r.IsDBNull(6)  ? null : r.GetDouble(6),
                Calorie    = r.IsDBNull(7)  ? null : r.GetDouble(7),
                AvgHr      = r.IsDBNull(8)  ? null : r.GetInt32(8),
                MaxHr      = r.IsDBNull(9)  ? null : r.GetInt32(9),
                MinHr      = r.IsDBNull(10) ? null : r.GetInt32(10),
                AvgSpo2    = r.IsDBNull(11) ? null : r.GetInt32(11),
                Sbp        = r.IsDBNull(12) ? null : r.GetInt32(12),
                Dbp        = r.IsDBNull(13) ? null : r.GetInt32(13),
                Fatigue    = r.IsDBNull(14) ? null : r.GetInt32(14),
                CreatedAt  = r.GetDateTime(15)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetLatestHealthSnapshot failed for {Device}", deviceId);
            return null;
        }
    }

    public int GetActiveWorkerCount()
    {
        try
        {
            using var conn = Open(); conn.Open();
            using var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM user_profiles WHERE is_active = 1", conn);
            return (int)cmd.ExecuteScalar()!;
        }
        catch (Exception ex) { _logger.LogError(ex, "GetActiveWorkerCount failed."); return 0; }
    }

    public IReadOnlyList<UserProfile> GetPagedUserProfiles(int skip, int take)
    {
        var list = new List<UserProfile>();
        try
        {
            using var conn = Open(); conn.Open();
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
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(MapUserProfile(r));
        }
        catch (Exception ex) { _logger.LogError(ex, "GetPagedUserProfiles failed."); }
        return list;
    }

    public int GetRecentAlarmCount(int withinHours)
    {
        try
        {
            using var conn = Open(); conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT COUNT(*)
                FROM alarms
                WHERE created_at >= DATEADD(HOUR, -@h, GETDATE())", conn);
            cmd.Parameters.AddWithValue("@h", withinHours);
            return (int)cmd.ExecuteScalar()!;
        }
        catch (Exception ex) { _logger.LogError(ex, "GetRecentAlarmCount failed."); return 0; }
    }

    public int GetRecentSosCount(int withinHours)
    {
        try
        {
            using var conn = Open(); conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT COUNT(*)
                FROM sos_events
                WHERE created_at >= DATEADD(HOUR, -@h, GETDATE())", conn);
            cmd.Parameters.AddWithValue("@h", withinHours);
            return (int)cmd.ExecuteScalar()!;
        }
        catch (Exception ex) { _logger.LogError(ex, "GetRecentSosCount failed."); return 0; }
    }

    public IReadOnlyList<AlarmEvent> GetRecentAlarms(int withinHours, int limit)
    {
        var list = new List<AlarmEvent>();
        try
        {
            using var conn = Open(); conn.Open();
            // Single round trip: JOIN resolves worker name; avoids loading all user profiles in the service layer
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
            using var r = cmd.ExecuteReader();
            while (r.Read())
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

    public (int TotalWorkers, int AlarmCount, int SosCount) GetDashboardCounts(int withinHours)
    {
        try
        {
            using var conn = Open(); conn.Open();
            // Single round trip replaces three separate COUNT queries
            using var cmd = new SqlCommand(@"
                SELECT
                    (SELECT COUNT(*) FROM user_profiles WHERE is_active = 1),
                    (SELECT COUNT(*) FROM alarms       WHERE created_at >= DATEADD(HOUR, -@h, GETDATE())),
                    (SELECT COUNT(*) FROM sos_events   WHERE created_at >= DATEADD(HOUR, -@h, GETDATE()))", conn);
            cmd.Parameters.AddWithValue("@h", withinHours);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return (0, 0, 0);
            return (r.GetInt32(0), r.GetInt32(1), r.GetInt32(2));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDashboardCounts failed.");
            return (0, 0, 0);
        }
    }

    public void Dispose() { }
}
