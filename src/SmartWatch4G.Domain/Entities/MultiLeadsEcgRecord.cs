using SmartWatch4G.Domain.Common;

namespace SmartWatch4G.Domain.Entities;

/// <summary>
/// Stores one Multi-Leads ECG data packet from a HisDataMultiLeadsECG packet.
/// Raw channel data is stored as base64 for compact storage.
/// </summary>
public sealed class MultiLeadsEcgRecord
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

    /// <summary>Number of ECG channels (leads).</summary>
    public int Channels { get; set; }

    /// <summary>Byte length per single data sample per channel.</summary>
    public int SampleByteLen { get; set; }

    /// <summary>Raw multi-lead ECG data encoded as base64.</summary>
    private string _rawDataBase64 = string.Empty;
    public string RawDataBase64
    {
        get => _rawDataBase64;
        set => _rawDataBase64 = Guard.NotNullOrWhiteSpace(value, nameof(RawDataBase64));
    }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
