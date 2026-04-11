using SmartWatch4G.Domain.Common;

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
    private string _dataTime = string.Empty;
    public string DataTime
    {
        get => _dataTime;
        set => _dataTime = Guard.NotNullOrWhiteSpace(value, nameof(DataTime));
    }

    /// <summary>X-axis samples serialised as a JSON int array.</summary>
    private string _xValuesJson = string.Empty;
    public string XValuesJson
    {
        get => _xValuesJson;
        set => _xValuesJson = Guard.NotNullOrWhiteSpace(value, nameof(XValuesJson));
    }

    /// <summary>Y-axis samples serialised as a JSON int array.</summary>
    private string _yValuesJson = string.Empty;
    public string YValuesJson
    {
        get => _yValuesJson;
        set => _yValuesJson = Guard.NotNullOrWhiteSpace(value, nameof(YValuesJson));
    }

    /// <summary>Z-axis samples serialised as a JSON int array.</summary>
    private string _zValuesJson = string.Empty;
    public string ZValuesJson
    {
        get => _zValuesJson;
        set => _zValuesJson = Guard.NotNullOrWhiteSpace(value, nameof(ZValuesJson));
    }

    public int SampleCount { get; set; }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
