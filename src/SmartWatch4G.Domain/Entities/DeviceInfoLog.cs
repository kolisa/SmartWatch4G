namespace SmartWatch4G.Domain.Entities;

public class DeviceInfoLog
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string RecordedAt { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? Version { get; set; }
    public string? WearingStatus { get; set; }
    public string? Signal { get; set; }
    public string? RawJson { get; set; }
    public DateTime CreatedAt { get; set; }
}
