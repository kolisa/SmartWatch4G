using SmartWatch4G.Application.DTOs;

namespace SmartWatch4G.Application.Interfaces;

/// <summary>
/// Application service for querying call logs and SOS events.
/// Controllers depend on this interface — no repository or DbContext enters the API layer.
/// </summary>
public interface ICallLogQueryService
{
    Task<IReadOnlyList<CallLogItemDto>> GetByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default);

    Task<IReadOnlyList<CallLogItemDto>> GetByRangeAsync(
        string deviceId, string from, string to, string? tz, CancellationToken ct = default);

    Task<IReadOnlyList<CallLogItemDto>> GetAllDevicesAndDateAsync(
        string date, string? tz, CancellationToken ct = default);
}
