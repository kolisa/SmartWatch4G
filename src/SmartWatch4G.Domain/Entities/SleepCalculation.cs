namespace SmartWatch4G.Domain.Entities;

public class SleepCalculation
{
    public string DeviceId { get; set; } = string.Empty;
    public string RecordDate { get; set; } = string.Empty;
    public int Completed { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public int Hr { get; set; }
    public int TurnTimes { get; set; }
    public double? RespAvg { get; set; }
    public double? RespMax { get; set; }
    public double? RespMin { get; set; }
    public string? Sections { get; set; }
    public int? DeepSleep { get; set; }
    public int? LightSleep { get; set; }
    public int? WeakSleep { get; set; }
    public int? EyemoveSleep { get; set; }
}
