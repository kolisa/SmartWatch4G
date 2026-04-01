using SmartWatch4G.Application.DTOs;

namespace SmartWatch4G.Application.Interfaces;

/// <summary>
/// Application service for querying GNSS/GPS track points.
/// Controllers depend on this interface — no repository or DbContext enters the API layer.
/// </summary>
public interface ILocationQueryService
{
    Task<IReadOnlyList<LocationPointDto>> GetByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default);

    Task<IReadOnlyList<LocationPointDto>> GetByRangeAsync(
        string deviceId, string from, string to, string? tz, CancellationToken ct = default);

    Task<IReadOnlyList<LocationPointDto>> GetRecentAsync(
        string deviceId, int minutes, string? tz, CancellationToken ct = default);

    Task<LocationPointDto?> GetLatestAsync(
        string deviceId, string? tz, CancellationToken ct = default);

    Task<IReadOnlyList<LocationPointDto>> GetLatestAllDevicesAsync(
        string? tz, CancellationToken ct = default);

    Task<IReadOnlyList<LocationPointDto>> GetAllDevicesAndDateAsync(
        string date, string? tz, CancellationToken ct = default);
}
