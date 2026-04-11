using SmartWatch4G.Domain.Common;

namespace SmartWatch4G.Domain.Entities;

/// <summary>
/// Stores a single GNSS (WGS-84) track-point uploaded by an OldMan (OM0) device.
/// </summary>
public sealed class GnssTrackRecord
{
    public int Id { get; set; }
    public string? DeviceId { get; set; }

    private string _trackTime = string.Empty;
    public string TrackTime
    {
        get => _trackTime;
        set => _trackTime = Guard.NotNullOrWhiteSpace(value, nameof(TrackTime));
    }

    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public int GpsType { get; set; }

    // Device health snapshot at track time
    public int? BatteryLevel { get; set; }
    public int? Rssi { get; set; }
    public long? Steps { get; set; }
    public float? DistanceMetres { get; set; }
    public float? CaloriesKcal { get; set; }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
