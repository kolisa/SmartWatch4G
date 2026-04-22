namespace SmartWatch4G.Domain.Entities;

public class HealthSnapshot
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string RecordTime { get; set; } = string.Empty;
    public int? Battery { get; set; }
    public int? Rssi { get; set; }
    public int? Steps { get; set; }
    public double? Distance { get; set; }
    public double? Calorie { get; set; }
    public int? AvgHr { get; set; }
    public int? MaxHr { get; set; }
    public int? MinHr { get; set; }
    public int? AvgSpo2 { get; set; }
    public int? Sbp { get; set; }
    public int? Dbp { get; set; }
    public int? Fatigue { get; set; }
    public DateTime CreatedAt { get; set; }
}
