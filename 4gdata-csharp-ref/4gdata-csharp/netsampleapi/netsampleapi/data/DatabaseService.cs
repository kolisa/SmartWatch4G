using Microsoft.Data.SqlClient;

namespace SampleApi.Data
{
    public class DatabaseService : IDisposable
    {
        private readonly string _connStr;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(IConfiguration config, ILogger<DatabaseService> logger)
        {
            _logger = logger;
            _connStr = config.GetConnectionString("SmartWatch")
                ?? throw new InvalidOperationException("Connection string 'SmartWatch' not found.");
            InitializeSchema();
        }

        private SqlConnection Open() => new SqlConnection(_connStr);

        private void InitializeSchema()
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
);";
            using var cmd = new SqlCommand(ddl, conn);
            cmd.ExecuteNonQuery();
        }

        // ── GPS ──────────────────────────────────────────────────────────────

        public void InsertGpsTrack(string deviceId, string gnssTime, double longitude, double latitude, string locType)
        {
            try
            {
                using var conn = Open(); conn.Open();
                using var cmd = new SqlCommand(@"
                    IF NOT EXISTS (SELECT 1 FROM gps_tracks WHERE device_id=@dev AND gnss_time=@t AND longitude=@lon AND latitude=@lat)
                    INSERT INTO gps_tracks (device_id, gnss_time, longitude, latitude, loc_type)
                    VALUES (@dev, @t, @lon, @lat, @type)", conn);
                cmd.Parameters.AddWithValue("@dev", deviceId);
                cmd.Parameters.AddWithValue("@t", gnssTime);
                cmd.Parameters.AddWithValue("@lon", longitude);
                cmd.Parameters.AddWithValue("@lat", latitude);
                cmd.Parameters.AddWithValue("@type", (object?)locType ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex) { _logger.LogError(ex, "InsertGpsTrack failed for {Device}", deviceId); }
        }

        // ── Health snapshots (battery, steps, HR, SpO2, BP, fatigue) ─────────

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

        // ── Alarms ────────────────────────────────────────────────────────────

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

        // ── SOS events ────────────────────────────────────────────────────────

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
                cmd.Parameters.AddWithValue("@lat",    (object?)lat         ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@lon",    (object?)lon         ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@num",    (object?)callNumber  ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@status", (object?)callStatus  ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@start",  (object?)callStart   ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@end",    (object?)callEnd     ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex) { _logger.LogError(ex, "InsertSosEvent failed for {Device}", deviceId); }
        }

        // ── Device info ───────────────────────────────────────────────────────

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

        public void Dispose() { }
    }
}
