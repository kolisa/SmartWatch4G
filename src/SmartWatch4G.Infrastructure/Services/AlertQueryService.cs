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

    public async Task<ServiceResult<IReadOnlyList<AlertSummaryResponse>>> GetRecentAlertsAsync(
        int withinHours = 24, int limit = 50)
    {
        if (withinHours < 1)   withinHours = 1;
        if (withinHours > 720) withinHours = 720;
        if (limit < 1)   limit = 1;
        if (limit > 500) limit = 500;

        try
        {
            var alarms = await _db.GetRecentAlarms(withinHours, limit);

            IReadOnlyList<AlertSummaryResponse> result = alarms
                .Select(a => new AlertSummaryResponse
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

            return ServiceResult<IReadOnlyList<AlertSummaryResponse>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRecentAlertsAsync failed");
            return ServiceResult<IReadOnlyList<AlertSummaryResponse>>.Fail("An unexpected error occurred.", 500);
        }
    }
}
