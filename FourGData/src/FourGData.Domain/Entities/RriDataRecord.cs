namespace SmartWatch4G.Domain.Entities;

/// <summary>
/// Stores one RRI (R-to-R interval) data packet for a device/timestamp.
/// RRI is continuous; combine all packets in a time-range for AF analysis.
/// </summary>
public sealed class RriDataRecord
{
    public int Id { get; set; }
    public string? DeviceId { get; set; }
    public string DataTime { get; set; } = string.Empty;
    public long Seq { get; set; }
    public int SampleCount { get; set; }

    /// <summary>
    /// RRI values (milliseconds) serialised as JSON array, e.g. [780, 802, 795, ...].
    /// </summary>
    public string RriValuesJson { get; set; } = string.Empty;

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
