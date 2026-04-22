namespace SmartWatch4G.Domain.Entities;

public class AlarmEvent
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string AlarmTime { get; set; } = string.Empty;
    public string AlarmType { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}
