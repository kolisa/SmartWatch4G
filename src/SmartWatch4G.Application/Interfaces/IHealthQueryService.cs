using SmartWatch4G.Application.DTOs;

namespace SmartWatch4G.Application.Interfaces;

/// <summary>
/// Application service for querying health snapshots and daily aggregates.
/// Controllers depend on this interface — no repository or DbContext enters the API layer.
/// </summary>
public interface IHealthQueryService
{
    Task<IReadOnlyList<HealthSnapshotDto>> GetSnapshotsByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default);

    Task<IReadOnlyList<HealthSnapshotDto>> GetSnapshotsByRangeAsync(
        string deviceId, string from, string to, string? tz, CancellationToken ct = default);

    Task<HealthSnapshotDto?> GetLatestSnapshotAsync(
        string deviceId, string? tz, CancellationToken ct = default);

    Task<HealthDailyStatsDto?> GetDailyStatsAsync(
        string deviceId, string date, CancellationToken ct = default);

    Task<IReadOnlyList<HealthSnapshotDto>> GetLatestSnapshotAllDevicesAsync(
        string? tz, CancellationToken ct = default);

    Task<IReadOnlyList<HealthDailyStatsDto>> GetDailyStatsAllDevicesAsync(
        string date, CancellationToken ct = default);
}
