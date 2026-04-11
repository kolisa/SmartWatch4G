using SmartWatch4G.Application.DTOs;

namespace SmartWatch4G.Application.Interfaces;

/// <summary>
/// Application service for querying PPG (photoplethysmography) data records.
/// </summary>
public interface IPpgQueryService
{
    Task<IReadOnlyList<PpgReadingDto>> GetByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default);
}
