using SmartWatch4G.Domain.Common;

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
    private string _sleepDate = string.Empty;
    public string SleepDate
    {
        get => _sleepDate;
        set => _sleepDate = Guard.NotNullOrWhiteSpace(value, nameof(SleepDate));
    }

    private string _dataTime = string.Empty;
    public string DataTime
    {
        get => _dataTime;
        set => _dataTime = Guard.NotNullOrWhiteSpace(value, nameof(DataTime));
    }

    public long Seq { get; set; }

    /// <summary>
    /// Compact JSON string representing one five-minute health slot used as
    /// input for sleep-stage calculation (e.g. {"Q":42,"T":[23,15],"E":{...}}).
    /// </summary>
    private string _sleepJson = string.Empty;
    public string SleepJson
    {
        get => _sleepJson;
        set => _sleepJson = Guard.NotNullOrWhiteSpace(value, nameof(SleepJson));
    }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
