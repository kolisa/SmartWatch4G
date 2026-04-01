using SmartWatch4G.Application.DTOs;

namespace SmartWatch4G.Application.Interfaces;

/// <summary>
/// Application service for querying RRI (R-to-R interval / HRV) readings.
/// Controllers depend on this interface — no repository or DbContext enters the API layer.
/// </summary>
public interface IRriQueryService
{
    Task<IReadOnlyList<RriReadingDto>> GetByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default);
}
