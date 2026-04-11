using SmartWatch4G.Application.DTOs;

namespace SmartWatch4G.Application.Interfaces;

/// <summary>
/// Application service for querying third-party device data records.
/// </summary>
public interface IThirdPartyDataQueryService
{
    Task<IReadOnlyList<ThirdPartyDataDto>> GetByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default);
}
