using SmartWatch4G.Application.DTOs;

namespace SmartWatch4G.Application.Interfaces;

/// <summary>
/// Application service for querying Multi-Leads ECG records.
/// </summary>
public interface IMultiLeadsEcgQueryService
{
    Task<IReadOnlyList<MultiLeadsEcgDto>> GetByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default);
}
