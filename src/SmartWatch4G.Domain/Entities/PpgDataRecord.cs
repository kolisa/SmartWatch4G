using SmartWatch4G.Domain.Common;

namespace SmartWatch4G.Domain.Entities;

/// <summary>
/// Stores one PPG (photoplethysmography) data packet from a HisDataPPG packet.
/// Raw samples are stored as a JSON int array for downstream analysis.
/// </summary>
public sealed class PpgDataRecord
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
    /// PPG sample pairs serialised as JSON. Each element is a two-element array [first, second]
    /// decoded from the raw int32 by splitting the high and low 16-bit words.
    /// </summary>
    private string _rawDataJson = string.Empty;
    public string RawDataJson
    {
        get => _rawDataJson;
        set => _rawDataJson = Guard.NotNullOrWhiteSpace(value, nameof(RawDataJson));
    }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
