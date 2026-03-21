using System.Text.Json.Serialization;

namespace SmartWatch4G.Domain.Entities;

/// <summary>
/// Stores one minute's worth of accelerometer (X/Y/Z) data from a wearable.
/// Combine a window of rows and pass to the iwown algo service
/// (<c>/calculation/parkinson/acc</c>) for Parkinson tremor scoring.
/// </summary>
public sealed class AccDataRecord
{
    public int Id { get; set; }
    public string? DeviceId { get; set; }

    /// <summary>Timestamp of this one-minute slot, e.g. "2024-03-15 14:01:00".</summary>
    public string DataTime { get; set; } = string.Empty;

    /// <summary>X-axis samples serialised as a JSON int array.</summary>
    public string XValuesJson { get; set; } = string.Empty;

    /// <summary>Y-axis samples serialised as a JSON int array.</summary>
    public string YValuesJson { get; set; } = string.Empty;

    /// <summary>Z-axis samples serialised as a JSON int array.</summary>
    public string ZValuesJson { get; set; } = string.Empty;

    public int SampleCount { get; set; }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
