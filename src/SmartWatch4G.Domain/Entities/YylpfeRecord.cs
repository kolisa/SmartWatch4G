using SmartWatch4G.Domain.Common;

namespace SmartWatch4G.Domain.Entities;

/// <summary>
/// Stores one decoded YYLPFE (PPG-based physiological feature) data packet.
/// Each packet contains multiple 12-byte structs; each is stored as one row.
/// </summary>
public sealed class YylpfeRecord
{
    public int Id { get; set; }
    public string? DeviceId { get; set; }

    /// <summary>Measurement timestamp derived from the base DataTime + offsetMs.</summary>
    private string _dataTime = string.Empty;
    public string DataTime
    {
        get => _dataTime;
        set => _dataTime = Guard.NotNullOrWhiteSpace(value, nameof(DataTime));
    }

    public long Seq { get; set; }

    /// <summary>PPG area above baseline (arbitrary units).</summary>
    public int AreaUp { get; set; }

    /// <summary>PPG area below baseline (arbitrary units).</summary>
    public int AreaDown { get; set; }

    /// <summary>R-to-R interval in milliseconds.</summary>
    public int Rri { get; set; }

    /// <summary>Motion/activity indicator (arbitrary units).</summary>
    public int Motion { get; set; }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
