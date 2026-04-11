using SmartWatch4G.Domain.Common;

namespace SmartWatch4G.Domain.Entities;

/// <summary>
/// Stores one ECG data packet for a device/timestamp.
/// Multiple packets sharing the same <see cref="DataTime"/> belong to
/// the same ECG measurement and must be combined before analysis.
/// </summary>
public sealed class EcgDataRecord
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
    /// Raw ECG samples serialised as a base64 string for compact storage.
    /// Decode to int[] before analysis.
    /// </summary>
    private string _rawDataBase64 = string.Empty;
    public string RawDataBase64
    {
        get => _rawDataBase64;
        set => _rawDataBase64 = Guard.NotNullOrWhiteSpace(value, nameof(RawDataBase64));
    }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
