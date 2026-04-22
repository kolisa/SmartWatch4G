using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Domain.Common;

namespace SmartWatch4G.Application.Interfaces;

public interface IAlertQueryService
{
    Task<ServiceResult<IReadOnlyList<AlarmSummaryResponse>>> GetRecentAlarmsAsync(int withinHours = 24, int limit = 50);
    Task<ServiceResult<FleetStatusResponse>> GetFleetStatusAsync();
}
