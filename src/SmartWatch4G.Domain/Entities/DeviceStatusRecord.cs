using SmartWatch4G.Domain.Common;

namespace SmartWatch4G.Domain.Entities;

/// <summary>
/// Persisted record of a device status notification.
/// </summary>
public sealed class DeviceStatusRecord
{
    public int Id { get; set; }

    private string _deviceId = string.Empty;
    public string DeviceId
    {
        get => _deviceId;
        set => _deviceId = Guard.NotNullOrWhiteSpace(value, nameof(DeviceId));
    }

    private string _eventTime = string.Empty;
    public string EventTime
    {
        get => _eventTime;
        set => _eventTime = Guard.NotNullOrWhiteSpace(value, nameof(EventTime));
    }

    private string _status = string.Empty;
    public string Status
    {
        get => _status;
        set => _status = Guard.NotNullOrWhiteSpace(value, nameof(Status));
    }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
