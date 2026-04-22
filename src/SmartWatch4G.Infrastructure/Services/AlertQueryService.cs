using Microsoft.Extensions.Logging;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Common;
using SmartWatch4G.Domain.Interfaces;

namespace SmartWatch4G.Infrastructure.Services;

public sealed class AlertQueryService : IAlertQueryService
{
    private readonly IDatabaseService _db;
    private readonly ILogger<AlertQueryService> _logger;

    public AlertQueryService(IDatabaseService db, ILogger<AlertQueryService> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public Task<ServiceResult<IReadOnlyList<AlarmSummaryResponse>>> GetRecentAlarmsAsync(
        int withinHours = 24, int limit = 50)
    {
        if (withinHours < 1)   withinHours = 1;
        if (withinHours > 720) withinHours = 720; // cap at 30 days
        if (limit < 1)   limit = 1;
        if (limit > 500) limit = 500;

        try
        {
            // GetRecentAlarms now JOINs user_profiles — no second query needed
            var alarms = _db.GetRecentAlarms(withinHours, limit);

            IReadOnlyList<AlarmSummaryResponse> result = alarms
                .Select(a => new AlarmSummaryResponse
                {
                    Id         = a.Id,
                    DeviceId   = a.DeviceId,
                    WorkerName = a.WorkerName,
                    AlarmTime  = a.AlarmTime,
                    AlarmType  = a.AlarmType,
                    Details    = a.Details,
                    CreatedAt  = a.CreatedAt
                })
                .ToList();

            return Task.FromResult(ServiceResult<IReadOnlyList<AlarmSummaryResponse>>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRecentAlarmsAsync failed");
            return Task.FromResult(
                ServiceResult<IReadOnlyList<AlarmSummaryResponse>>.Fail("An unexpected error occurred.", 500));
        }
    }

    public Task<ServiceResult<FleetStatusResponse>> GetFleetStatusAsync()
    {
        try
        {
            var (workers, alarms, sos) = _db.GetDashboardCounts(24);

            return Task.FromResult(ServiceResult<FleetStatusResponse>.Ok(new FleetStatusResponse
            {
                TotalWorkers = workers,
                ActiveAlerts = alarms,
                SosCount     = sos
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFleetStatusAsync failed");
            return Task.FromResult(
                ServiceResult<FleetStatusResponse>.Fail("An unexpected error occurred.", 500));
        }
    }
}
