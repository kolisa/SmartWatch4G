namespace SmartWatch4G.Domain.Entities;

/// <summary>
/// Persisted health measurement parsed from a device history-data packet.
/// Each row stores one time-stamped health snapshot from the wearable.
/// </summary>
public sealed class HealthDataRecord
{
    public int Id { get; set; }
    public string? DeviceId { get; set; }
    public string DataTime { get; set; } = string.Empty;
    public long Seq { get; set; }

    // Steps / activity
    public uint? Steps { get; set; }
    public float? DistanceMetres { get; set; }
    public float? CaloriesKcal { get; set; }
    public uint? ActivityType { get; set; }
    public uint? ActivityState { get; set; }

    // Heart rate
    public uint? AvgHeartRate { get; set; }
    public uint? MaxHeartRate { get; set; }
    public uint? MinHeartRate { get; set; }

    // SpO2 (blood oxygen)
    public uint? AvgSpo2 { get; set; }
    public uint? MaxSpo2 { get; set; }
    public uint? MinSpo2 { get; set; }

    // Blood pressure
    public uint? Sbp { get; set; }
    public uint? Dbp { get; set; }

    // HRV
    public double? HrvSdnn { get; set; }
    public double? HrvRmssd { get; set; }
    public double? HrvPnn50 { get; set; }
    public double? HrvMean { get; set; }
    public int? Fatigue { get; set; }

    // Temperature
    public float? AxillaryTemp { get; set; }
    public float? EstimatedTemp { get; set; }
    public float? ShellTemp { get; set; }
    public float? EnvTemp { get; set; }

    // Sleep raw bytes (base64-encoded payload for later calculation)
    public string? SleepDataJson { get; set; }

    // Bioz / body composition
    public int? BiozR { get; set; }
    public int? BiozX { get; set; }
    public float? BodyFat { get; set; }
    public float? Bmi { get; set; }

    // Blood sugar / potassium
    public float? BloodSugar { get; set; }
    public float? BloodPotassium { get; set; }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
