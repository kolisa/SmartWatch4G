namespace SmartWatch4G.Domain.Entities;

/// <summary>
/// Persisted record of a device status notification.
/// </summary>
public sealed class DeviceStatusRecord
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string EventTime { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
