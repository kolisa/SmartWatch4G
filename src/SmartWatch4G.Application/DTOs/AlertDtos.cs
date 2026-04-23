namespace SmartWatch4G.Application.DTOs;

public sealed class AlertSummaryResponse
{
    public int Id { get; init; }
    public string DeviceId { get; init; } = string.Empty;
    public string? WorkerName { get; init; }
    public string AlarmTime { get; init; } = string.Empty;
    public string AlarmType { get; init; } = string.Empty;
    public string? Details { get; init; }
    public DateTime CreatedAt { get; init; }
}
