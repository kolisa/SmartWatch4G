using SmartWatch4G.Application.DTOs;

namespace SmartWatch4G.Application.Interfaces;

/// <summary>
/// Application service for querying device alarm events.
/// Controllers depend on this interface — no repository or DbContext enters the API layer.
/// </summary>
public interface IAlarmQueryService
{
    Task<IReadOnlyList<AlarmEventDto>> GetByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default);

    Task<IReadOnlyList<AlarmEventDto>> GetByRangeAsync(
        string deviceId, string from, string to, string? tz, CancellationToken ct = default);

    Task<AlarmEventDto?> GetLatestAsync(
        string deviceId, string? tz, CancellationToken ct = default);

    Task<IReadOnlyList<AlarmEventDto>> GetLatestAllDevicesAsync(
        string? tz, CancellationToken ct = default);

    Task<IReadOnlyList<AlarmEventDto>> GetAllDevicesAndDateAsync(
        string date, string? tz, CancellationToken ct = default);
}
