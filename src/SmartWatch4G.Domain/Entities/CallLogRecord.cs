using SmartWatch4G.Domain.Common;

namespace SmartWatch4G.Domain.Entities;

/// <summary>
/// Persisted call-log entry.  Both normal call logs and SOS alarm logs
/// are stored in this table; <see cref="IsSosAlarm"/> distinguishes them.
/// </summary>
public sealed class CallLogRecord
{
    public int Id { get; set; }

    private string _deviceId = string.Empty;
    public string DeviceId
    {
        get => _deviceId;
        set => _deviceId = Guard.NotNullOrWhiteSpace(value, nameof(DeviceId));
    }

    // -- call fields --
    public int CallStatus { get; set; }
    public string? CallNumber { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }

    // -- SOS fields (populated only when IsSosAlarm = true) --
    public bool IsSosAlarm { get; set; }
    public string? AlarmTime { get; set; }
    public string? AlarmLat { get; set; }
    public string? AlarmLon { get; set; }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
