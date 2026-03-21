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
    public string DataTime { get; set; } = string.Empty;
    public long Seq { get; set; }
    public int SampleCount { get; set; }

    /// <summary>
    /// Raw ECG samples serialised as a base64 string for compact storage.
    /// Decode to int[] before analysis.
    /// </summary>
    public string RawDataBase64 { get; set; } = string.Empty;

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
