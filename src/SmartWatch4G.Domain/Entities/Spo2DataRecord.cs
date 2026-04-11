using SmartWatch4G.Domain.Common;

namespace SmartWatch4G.Domain.Entities;

/// <summary>
/// Stores one SPO2 sample-point from a HisDataSpo2 packet.
/// Combine all rows for a device over the desired time range and pass to
/// the iwown algo service (<c>/calculation/spo2</c>) for OSAHS-risk scoring.
/// </summary>
public sealed class Spo2DataRecord
{
    public int Id { get; set; }
    public string? DeviceId { get; set; }

    private string _dataTime = string.Empty;
    public string DataTime
    {
        get => _dataTime;
        set => _dataTime = Guard.NotNullOrWhiteSpace(value, nameof(DataTime));
    }

    /// <summary>SPO2 percentage value (0-100).</summary>
    public int Spo2 { get; set; }

    /// <summary>Heart rate at the same sample point.</summary>
    public int HeartRate { get; set; }

    /// <summary>Perfusion index byte.</summary>
    public int Perfusion { get; set; }

    /// <summary>Touch/contact flag byte.</summary>
    public int Touch { get; set; }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
