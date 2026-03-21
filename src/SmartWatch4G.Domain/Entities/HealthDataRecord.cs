namespace SmartWatch4G.Domain.Entities;

/// <summary>
/// Persisted health measurement parsed from a device history-data packet (opcode 0x80).
/// Each row stores one time-stamped health snapshot from the wearable.
/// All values cover one minute of measurement per the iwown protocol.
/// </summary>
public sealed class HealthDataRecord
{
    public int Id { get; set; }
    public string? DeviceId { get; set; }
    public string DataTime { get; set; } = string.Empty;
    public long Seq { get; set; }

    // Steps / activity
    public long? Steps { get; set; }
    public float? DistanceMetres { get; set; }
    public float? CaloriesKcal { get; set; }
    public long? ActivityType { get; set; }
    public long? ActivityState { get; set; }

    // Heart rate
    public long? AvgHeartRate { get; set; }
    public long? MaxHeartRate { get; set; }
    public long? MinHeartRate { get; set; }

    // SpO2 (blood oxygen)
    public long? AvgSpo2 { get; set; }
    public long? MaxSpo2 { get; set; }
    public long? MinSpo2 { get; set; }

    // Blood pressure
    public long? Sbp { get; set; }
    public long? Dbp { get; set; }

    // HRV / stress
    public double? HrvSdnn { get; set; }
    public double? HrvRmssd { get; set; }
    public double? HrvPnn50 { get; set; }
    public double? HrvMean { get; set; }
    public int? Fatigue { get; set; }

    // Body temperature
    // TemperatureIsValid: 1 = algorithm done / value usable; 0 = algorithm still computing
    public int? TemperatureIsValid { get; set; }
    public float? AxillaryTemp { get; set; }
    public float? EstimatedTemp { get; set; }
    public float? ShellTemp { get; set; }
    public float? EnvTemp { get; set; }

    // Mattress temperature / humidity (HumitureData — bed sensor)
    public float? MatressTemperature { get; set; }
    public float? MatressHumidity { get; set; }

    // Sleep raw bytes (base64-encoded payload for later calculation)
    public string? SleepDataJson { get; set; }

    // Bioz / body composition
    public int? BiozR { get; set; }
    public int? BiozX { get; set; }
    public float? BodyFat { get; set; }
    public float? Bmi { get; set; }

    // Blood chemistry
    public float? BloodSugar { get; set; }
    public float? BloodPotassium { get; set; }

    // Blood pressure heart rate (bp_bpm_data)
    public long? BpBpm { get; set; }

    // Uric acid (direct wearable measurement)
    public long? UricAcid { get; set; }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
