using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Domain.Common;

namespace SmartWatch4G.Application.Interfaces;

public interface IDeviceConfigQueryService
{
    /// <summary>Command configurations for all devices in a company, paginated.</summary>
    Task<ServiceResult<DeviceConfigPagedResult>> GetByCompanyAsync(int companyId, int page, int pageSize);

    /// <summary>Command configuration for a single device.</summary>
    Task<ServiceResult<DeviceConfigResponse>> GetByDeviceAsync(string deviceId);
}
