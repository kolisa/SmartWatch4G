namespace SmartWatch4G.Application.DTOs;

// ── Generic response envelope ──────────────────────────────────────────────────

/// <summary>
/// Wraps a list of items returned by the app-facing query API.
/// </summary>
public sealed class ApiListResponse<T>
{
    public int ReturnCode { get; set; }
    public int Count { get; set; }
    public IReadOnlyList<T> Data { get; set; } = [];
}

/// <summary>
/// Wraps a single item returned by the app-facing query API.
/// </summary>
public sealed class ApiItemResponse<T>
{
    public int ReturnCode { get; set; }
    public T? Data { get; set; }
}

// ── Device ────────────────────────────────────────────────────────────────────

/// <summary>Summary row used in the device list endpoint.</summary>
public sealed class DeviceSummaryDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string WearingStatus { get; set; } = string.Empty;
    public string NetworkStatus { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}

/// <summary>Full device information returned by the device detail endpoint.</summary>
public sealed class DeviceDetailDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string Imsi { get; set; } = string.Empty;
    public string Sn { get; set; } = string.Empty;
    public string Mac { get; set; } = string.Empty;
    public string NetType { get; set; } = string.Empty;
    public string NetOperator { get; set; } = string.Empty;
    public string WearingStatus { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Sim1IccId { get; set; } = string.Empty;
    public string Sim1CellId { get; set; } = string.Empty;
    public string Sim1NetAdhere { get; set; } = string.Empty;
    public string NetworkStatus { get; set; } = string.Empty;
    public string BandDetail { get; set; } = string.Empty;
    public string RefSignal { get; set; } = string.Empty;
    public string Band { get; set; } = string.Empty;
    public string CommunicationMode { get; set; } = string.Empty;
    public int WatchEvent { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}

/// <summary>Single device-status event.</summary>
public sealed class DeviceStatusItemDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string EventTime { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ReceivedAt { get; set; } = string.Empty;
}

// ── Health ────────────────────────────────────────────────────────────────────

/// <summary>One-minute health snapshot (heart rate, steps, BP, temperature, etc.).</summary>
public sealed class HealthSnapshotDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string DataTime { get; set; } = string.Empty;
    public long? Steps { get; set; }
    public float? DistanceMetres { get; set; }
    public float? CaloriesKcal { get; set; }
    public long? ActivityType { get; set; }
    public long? ActivityState { get; set; }
    public long? AvgHeartRate { get; set; }
    public long? MaxHeartRate { get; set; }
    public long? MinHeartRate { get; set; }
    public long? AvgSpo2 { get; set; }
    public long? Sbp { get; set; }
    public long? Dbp { get; set; }
    public double? HrvSdnn { get; set; }
    public double? HrvRmssd { get; set; }
    public double? HrvPnn50 { get; set; }
    public double? HrvMean { get; set; }
    public int? Fatigue { get; set; }
    public float? AxillaryTemp { get; set; }
    public float? EstimatedTemp { get; set; }
    public float? BodyFat { get; set; }
    public float? Bmi { get; set; }
    public float? BloodSugar { get; set; }
    public float? BloodPotassium { get; set; }
}

/// <summary>Single SpO2 (blood-oxygen) reading.</summary>
public sealed class Spo2ReadingDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string DataTime { get; set; } = string.Empty;
    public int Spo2 { get; set; }
    public int HeartRate { get; set; }
    public int Perfusion { get; set; }
    public int Touch { get; set; }
}

/// <summary>ECG waveform record (raw data is base-64 encoded).</summary>
public sealed class EcgRecordDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string DataTime { get; set; } = string.Empty;
    public long Seq { get; set; }
    public int SampleCount { get; set; }
    public string RawDataBase64 { get; set; } = string.Empty;
}

/// <summary>RRI (R-to-R interval / HRV) reading.</summary>
public sealed class RriReadingDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string DataTime { get; set; } = string.Empty;
    public long Seq { get; set; }
    public int SampleCount { get; set; }
    public string RriValuesJson { get; set; } = string.Empty;
}

// ── Activity ──────────────────────────────────────────────────────────────────

/// <summary>Single GPS track point.</summary>
public sealed class LocationPointDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string TrackTime { get; set; } = string.Empty;
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public int GpsType { get; set; }
    public int? BatteryLevel { get; set; }
    public int? Rssi { get; set; }
    public long? Steps { get; set; }
    public float? DistanceMetres { get; set; }
    public float? CaloriesKcal { get; set; }
}

/// <summary>Single accelerometer sample burst.</summary>
public sealed class AccelerometerReadingDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string DataTime { get; set; } = string.Empty;
    public int SampleCount { get; set; }
    public string XValuesJson { get; set; } = string.Empty;
    public string YValuesJson { get; set; } = string.Empty;
    public string ZValuesJson { get; set; } = string.Empty;
}

// ── Alarms ────────────────────────────────────────────────────────────────────

/// <summary>Single alarm event raised by a device.</summary>
public sealed class AlarmEventDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string AlarmType { get; set; } = string.Empty;
    public string AlarmTime { get; set; } = string.Empty;
    public double? Value1 { get; set; }
    public double? Value2 { get; set; }
    public string? Notes { get; set; }
    public string ReceivedAt { get; set; } = string.Empty;
}

// ── Health aggregates ─────────────────────────────────────────────────────────

/// <summary>Aggregated daily health statistics computed from 1-minute snapshots.</summary>
public sealed class HealthDailyStatsDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public int RecordCount { get; set; }
    public long? AvgHeartRate { get; set; }
    public long? MaxHeartRate { get; set; }
    public long? MinHeartRate { get; set; }
    public long? AvgSpo2 { get; set; }
    public long? MinSpo2 { get; set; }
    public long? TotalSteps { get; set; }
    public float? TotalDistanceMetres { get; set; }
    public float? TotalCaloriesKcal { get; set; }
    public float? AvgAxillaryTemp { get; set; }
    public long? AvgSbp { get; set; }
    public long? AvgDbp { get; set; }
    public double? AvgHrvSdnn { get; set; }
    public int? AvgFatigue { get; set; }
}

// ── Sleep trend ───────────────────────────────────────────────────────────────

/// <summary>One entry per day in a multi-day sleep trend query.</summary>
public sealed class SleepTrendItemDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string SleepDate { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public int TotalSleepMinutes { get; set; }
    public int DeepSleep { get; set; }
    public int LightSleep { get; set; }
    public int WeakSleep { get; set; }
    public int EyeMoveSleep { get; set; }
    public int Score { get; set; }
    public int OsahsRisk { get; set; }
    public int Spo2Score { get; set; }
    public int SleepHeartRate { get; set; }
}

// ── Call logs ─────────────────────────────────────────────────────────────────

/// <summary>Single call-log entry (normal or SOS alarm).</summary>
public sealed class CallLogItemDto
{
    public string DeviceId { get; set; } = string.Empty;
    public int CallStatus { get; set; }
    public string? CallNumber { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public bool IsSosAlarm { get; set; }
    public string? AlarmTime { get; set; }
    public string? AlarmLat { get; set; }
    public string? AlarmLon { get; set; }
    public string ReceivedAt { get; set; } = string.Empty;
}
