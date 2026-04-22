namespace SmartWatch4G.Domain.Entities;

public class SosEvent
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string AlarmTime { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? CallNumber { get; set; }
    public int? CallStatus { get; set; }
    public string? CallStart { get; set; }
    public string? CallEnd { get; set; }
    public DateTime CreatedAt { get; set; }
}
