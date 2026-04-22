namespace SmartWatch4G.Application.DTOs;

public sealed class AlarmSummaryResponse
{
    public int Id { get; init; }
    public string DeviceId { get; init; } = string.Empty;
    public string? WorkerName { get; init; }
    public string AlarmTime { get; init; } = string.Empty;
    public string AlarmType { get; init; } = string.Empty;
    public string? Details { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class FleetStatusResponse
{
    public int TotalWorkers { get; init; }
    public int ActiveAlerts { get; init; }
    public int SosCount { get; init; }
}
