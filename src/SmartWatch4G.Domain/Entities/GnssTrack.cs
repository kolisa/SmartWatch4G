namespace SmartWatch4G.Domain.Entities;

public class GnssTrack
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string GnssTime { get; set; } = string.Empty;
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public string? LocType { get; set; }
    public DateTime CreatedAt { get; set; }
}
