using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Domain.Common;

namespace SmartWatch4G.Application.Interfaces;

public interface IDashboardService
{
    Task<ServiceResult<DashboardSummaryResponse>> GetSummaryAsync();
}
