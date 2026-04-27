using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Domain.Common;

namespace SmartWatch4G.Application.Interfaces;

public interface IGpsQueryService
{
    /// <summary>All GPS tracks for a company, paginated and filterable. Includes online/offline counts.</summary>
    Task<ServiceResult<GpsPagedResult>> GetByCompanyAsync(int companyId, GpsQueryParams q);

    /// <summary>Only devices currently online, latest GPS position each.</summary>
    Task<ServiceResult<GpsPagedResult>> GetOnlineByCompanyAsync(int companyId, GpsQueryParams q);

    /// <summary>Only devices currently offline, latest GPS position each.</summary>
    Task<ServiceResult<GpsPagedResult>> GetOfflineByCompanyAsync(int companyId, GpsQueryParams q);

    /// <summary>GPS track history for a single device.</summary>
    Task<ServiceResult<GpsPagedResult>> GetByDeviceAsync(string deviceId, GpsQueryParams q);

    /// <summary>Online/offline status and latest GPS position for a single device.</summary>
    Task<ServiceResult<DeviceGpsStatusResponse>> GetDeviceGpsStatusAsync(string deviceId);

    /// <summary>
    /// All GPS tracks + latest health snapshot for every device in a company on a given date.
    /// Defaults to today when <paramref name="date"/> is null.
    /// Tracks are ordered newest-first so index 0 is the current position.
    /// </summary>
    Task<ServiceResult<IReadOnlyList<DeviceMapResponse>>> GetMapDataAsync(int companyId, System.DateTime? date);

    /// <summary>
    /// All GPS tracks + latest health snapshot for a single device on a given date.
    /// Defaults to today when <paramref name="date"/> is null.
    /// Tracks are ordered newest-first so index 0 is the current position.
    /// </summary>
    Task<ServiceResult<DeviceMapResponse>> GetDeviceMapDataAsync(string deviceId, System.DateTime? date);
}
