namespace SmartWatch4G.Domain.Entities;

/// <summary>
/// Stores the pre-processed sleep string (JSON compact) for a given device / time-slot.
/// All rows for the same deviceid + date are combined by the sleep-calculation engine
/// to produce the final sleep result.
/// </summary>
public sealed class SleepDataRecord
{
    public int Id { get; set; }
    public string? DeviceId { get; set; }

    /// <summary>Date component of the measurement, e.g. "2024-03-15".</summary>
    public string SleepDate { get; set; } = string.Empty;

    public string DataTime { get; set; } = string.Empty;
    public long Seq { get; set; }

    /// <summary>
    /// Compact JSON string representing one five-minute health slot used as
    /// input for sleep-stage calculation (e.g. {"Q":42,"T":[23,15],"E":{...}}).
    /// </summary>
    public string SleepJson { get; set; } = string.Empty;

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
