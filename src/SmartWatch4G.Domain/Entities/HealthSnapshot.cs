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
    public double? BodyTempEvi { get; set; }
    public int? BodyTempEsti { get; set; }
    public int? TempType { get; set; }
    public int? BpBpm { get; set; }
    public double? BloodPotassium { get; set; }
    public double? BloodSugar { get; set; }
    public double? BiozR { get; set; }
    public double? BiozX { get; set; }
    public double? BiozFat { get; set; }
    public double? BiozBmi { get; set; }
    public int? BiozType { get; set; }
    public double? BreathRate { get; set; }
    public int? MoodLevel { get; set; }
    public DateTime CreatedAt { get; set; }
}
