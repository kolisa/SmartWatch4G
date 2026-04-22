namespace SmartWatch4G.Domain.Entities;

public class AlarmEvent
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    /// <summary>Populated by JOIN queries; null when loaded without user_profiles join.</summary>
    public string? WorkerName { get; set; }
    public string AlarmTime { get; set; } = string.Empty;
    public string AlarmType { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}
