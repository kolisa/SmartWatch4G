namespace SmartWatch4G.Domain.Entities;

/// <summary>
/// Persisted alarm event received from a wearable device.
/// </summary>
public sealed class AlarmEventRecord
{
    public int Id { get; set; }
    public string? DeviceId { get; set; }

    /// <summary>
    /// Alarm category, e.g. "HR", "SPO2", "FALL", "SOS", "BP", "TEMPERATURE",
    /// "THROMBUS", "LOW_POWER", "POWER_OFF", "NOT_WEARING", "PHONE_INTERCEPT",
    /// "BLOOD_POTASSIUM", "BLOOD_SUGAR".
    /// </summary>
    public string AlarmType { get; set; } = string.Empty;

    public string AlarmTime { get; set; } = string.Empty;

    /// <summary>Primary numeric value (e.g. HR bpm, SpO2 %, Sbp, temperature).</summary>
    public double? Value1 { get; set; }

    /// <summary>Secondary numeric value (e.g. Dbp).</summary>
    public double? Value2 { get; set; }

    /// <summary>Free-form extra detail (phone number for intercept alarm, etc.).</summary>
    public string? Notes { get; set; }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
