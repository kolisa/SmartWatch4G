using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Domain.Common;

namespace SmartWatch4G.Application.Interfaces;

public interface IDashboardService
{
    Task<ServiceResult<DashboardStatsResponse>> GetStatsAsync(int? companyId, int onlineCount, int offlineCount);

    /// <summary>Legacy summary used by FleetController.</summary>
    Task<ServiceResult<DashboardSummaryResponse>> GetSummaryAsync(int? companyId = null);
}
