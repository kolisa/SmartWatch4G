using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Domain.Common;

namespace SmartWatch4G.Application.Interfaces;

public interface IUserProfileQueryService
{
    Task<ServiceResult<PagedResult<UserProfileSummaryResponse>>> GetPagedUserProfilesAsync(int page, int pageSize, int? companyId = null);
    Task<ServiceResult<UserProfileDetailResponse>> GetUserProfileDetailAsync(string deviceId);
    Task<ServiceResult<DeviceTelemetryResponse>> GetDeviceTelemetryAsync(string deviceId);
    Task<ServiceResult<IReadOnlyList<DeviceTelemetryResponse>>> GetAllDeviceTelemetryAsync(int? companyId = null);
}
