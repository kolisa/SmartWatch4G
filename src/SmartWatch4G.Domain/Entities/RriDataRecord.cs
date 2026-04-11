using SmartWatch4G.Domain.Common;

namespace SmartWatch4G.Domain.Entities;

/// <summary>
/// Stores one RRI (R-to-R interval) data packet for a device/timestamp.
/// RRI is continuous; combine all packets in a time-range for AF analysis.
/// </summary>
public sealed class RriDataRecord
{
    public int Id { get; set; }
    public string? DeviceId { get; set; }

    private string _dataTime = string.Empty;
    public string DataTime
    {
        get => _dataTime;
        set => _dataTime = Guard.NotNullOrWhiteSpace(value, nameof(DataTime));
    }

    public long Seq { get; set; }
    public int SampleCount { get; set; }

    /// <summary>
    /// RRI values (milliseconds) serialised as JSON array, e.g. [780, 802, 795, ...].
    /// </summary>
    private string _rriValuesJson = string.Empty;
    public string RriValuesJson
    {
        get => _rriValuesJson;
        set => _rriValuesJson = Guard.NotNullOrWhiteSpace(value, nameof(RriValuesJson));
    }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
