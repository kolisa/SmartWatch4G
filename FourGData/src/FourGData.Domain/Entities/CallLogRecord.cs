namespace SmartWatch4G.Domain.Entities;

/// <summary>
/// Persisted call-log entry.  Both normal call logs and SOS alarm logs
/// are stored in this table; <see cref="IsSosAlarm"/> distinguishes them.
/// </summary>
public sealed class CallLogRecord
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;

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
