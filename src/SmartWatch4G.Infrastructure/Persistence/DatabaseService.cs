using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    updated_at     DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_fall_settings')
CREATE TABLE device_fall_settings (
    device_id       NVARCHAR(50) PRIMARY KEY,
    fall_check      BIT          NULL,
    fall_threshold  INT          NULL,
    updated_at      DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_data_freq')
CREATE TABLE device_data_freq (
    device_id          NVARCHAR(50) PRIMARY KEY,
    gps_auto_check     BIT          NULL,
    gps_interval_time  INT          NULL,
    power_mode         INT          NULL,
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
    updated_at           DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_lcd_gesture')
CREATE TABLE device_lcd_gesture (
    device_id  NVARCHAR(50) PRIMARY KEY,
    open       BIT          NULL,
    start_hour INT          NULL,
    end_hour   INT          NULL,
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
    updated_at DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_spo2_alarm')
CREATE TABLE device_spo2_alarm (
    device_id  NVARCHAR(50) PRIMARY KEY,
    open       BIT          NULL,
    low        INT          NULL,
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
    updated_at DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_temp_alarm')
CREATE TABLE device_temp_alarm (
    device_id  NVARCHAR(50) PRIMARY KEY,
    open       BIT          NULL,
    high       INT          NULL,
    low        INT          NULL,
    updated_at DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_auto_af')
CREATE TABLE device_auto_af (
    device_id       NVARCHAR(50) PRIMARY KEY,
    open            BIT          NULL,
    interval        INT          NULL,
    rri_single_time BIT          NULL,
    rri_type        INT          NULL,
    updated_at      DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_goal')
CREATE TABLE device_goal (
    device_id  NVARCHAR(50) PRIMARY KEY,
    step       INT          NULL,
    distance   INT          NULL,
    calorie    INT          NULL,
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
    updated_at       DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_bp_adjust')
CREATE TABLE device_bp_adjust (
    device_id  NVARCHAR(50) PRIMARY KEY,
    sbp_band   INT          NULL,
    dbp_band   INT          NULL,
    sbp_meter  INT          NULL,
    dbp_meter  INT          NULL,
    updated_at DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_hr_interval')
CREATE TABLE device_hr_interval (
    device_id  NVARCHAR(50) PRIMARY KEY,
    interval   INT          NULL,
    updated_at DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_other_interval')
CREATE TABLE device_other_interval (
    device_id  NVARCHAR(50) PRIMARY KEY,
    interval   INT          NULL,
    updated_at DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_gps_settings')
CREATE TABLE device_gps_settings (
    device_id         NVARCHAR(50) PRIMARY KEY,
    gps_auto_check    BIT          NULL,
    gps_interval_time INT          NULL,
    run_gps           BIT          NULL,
    updated_at        DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'device_phonebook')
CREATE TABLE device_phonebook (
    id         INT IDENTITY(1,1) PRIMARY KEY,
    device_id  NVARCHAR(50)  NOT NULL,
    name       NVARCHAR(100) NOT NULL,
    number     NVARCHAR(30)  NOT NULL,
    sos        BIT           NOT NULL DEFAULT 0,
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
    created_at DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'af_calculations')
CREATE TABLE af_calculations (
    id         INT IDENTITY(1,1) PRIMARY KEY,
    device_id  NVARCHAR(50) NOT NULL,
    result     INT          NOT NULL,
    created_at DATETIME2    DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'spo2_calculations')
CREATE TABLE spo2_calculations (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    device_id   NVARCHAR(50) NOT NULL,
    spo2_score  FLOAT        NOT NULL,
    osahs_risk  INT          NULL,
    created_at  DATETIME2    DEFAULT GETDATE()
);";
            using var cmd = new SqlCommand(ddl, conn);
            cmd.ExecuteNonQuery();
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
                IF NOT EXISTS (SELECT 1 FROM gps_tracks WHERE device_id=@dev AND gnss_time=@t AND longitude=@lon AND latitude=@lat)
                INSERT INTO gps_tracks (device_id, gnss_time, longitude, latitude, loc_type)
                VALUES (@dev, @t, @lon, @lat, @type)", conn);
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
                USING (SELECT @dev AS device_id, @rt AS record_time) AS s
                    ON t.device_id = s.device_id AND t.record_time = s.record_time
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
                    fatigue  = COALESCE(@fat,  t.fatigue)
                WHEN NOT MATCHED THEN INSERT
                    (device_id, record_time, battery, rssi, steps, distance, calorie, avg_hr, max_hr, min_hr, avg_spo2, sbp, dbp, fatigue)
                    VALUES (@dev, @rt, @bat, @rssi, @stp, @dist, @cal, @ahr, @xhr, @nhr, @spo, @sbp, @dbp, @fat);", conn);

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
                IF NOT EXISTS (SELECT 1 FROM alarms WHERE device_id=@dev AND alarm_time=@t AND alarm_type=@type)
                INSERT INTO alarms (device_id, alarm_time, alarm_type, details)
                VALUES (@dev, @t, @type, @det)", conn);
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
                IF NOT EXISTS (SELECT 1 FROM sos_events WHERE device_id=@dev AND alarm_time=@t)
                INSERT INTO sos_events (device_id, alarm_time, latitude, longitude, call_number, call_status, call_start, call_end)
                VALUES (@dev, @t, @lat, @lon, @num, @status, @start, @end)", conn);
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
                INSERT INTO device_info_log (device_id, recorded_at, model, version, wearing_status, signal, raw_json)
                VALUES (@dev, @rat, @model, @ver, @wear, @sig, @json)", conn);
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
                    (device_id,record_date,completed,start_time,end_time,hr,turn_times,resp_avg,resp_max,resp_min,sections)
                VALUES (@dev,@rd,@comp,@st,@et,@hr,@tt,@ra,@rx,@rn,@sec)", conn);
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
                INSERT INTO ecg_calculations (device_id,result,hr,effective,direction)
                VALUES (@dev,@res,@hr,@eff,@dir)", conn);
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
                INSERT INTO af_calculations (device_id,result) VALUES (@dev,@res)", conn);
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
                INSERT INTO spo2_calculations (device_id,spo2_score,osahs_risk)
                VALUES (@dev,@score,@risk)", conn);
            cmd.Parameters.AddWithValue("@dev",   deviceId);
            cmd.Parameters.AddWithValue("@score", spo2Score);
            cmd.Parameters.AddWithValue("@risk",  (object?)oshahsRisk ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { _logger.LogError(ex, "InsertSpo2Calculation failed for {Device}", deviceId); }
    }

    public void Dispose() { }
}
