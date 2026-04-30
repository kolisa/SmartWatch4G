using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Domain.Common;

namespace SmartWatch4G.Application.Interfaces;

public interface IDashboardService
{
    /// <summary>
    /// Returns real-time dashboard stats by querying the Iwown device/status API
    /// for every active device in parallel.
    /// </summary>
    Task<ServiceResult<DashboardStatsResponse>> GetStatsAsync(int? companyId = null);

    /// <summary>Legacy summary used by FleetController.</summary>
    Task<ServiceResult<DashboardSummaryResponse>> GetSummaryAsync(int? companyId = null);
}
