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
}
