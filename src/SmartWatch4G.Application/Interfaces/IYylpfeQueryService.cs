using SmartWatch4G.Application.DTOs;

namespace SmartWatch4G.Application.Interfaces;

/// <summary>
/// Application service for querying YYLPFE physiological feature records.
/// </summary>
public interface IYylpfeQueryService
{
    Task<IReadOnlyList<YylpfeReadingDto>> GetByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default);
}
