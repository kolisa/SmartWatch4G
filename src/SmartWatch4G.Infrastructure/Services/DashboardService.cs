using Microsoft.Extensions.Logging;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Common;
using SmartWatch4G.Domain.Interfaces;

namespace SmartWatch4G.Infrastructure.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly IDatabaseService _db;
    private readonly ILogger<DashboardService> _logger;

    private const int AlertWindowHours = 24;

    public DashboardService(IDatabaseService db, ILogger<DashboardService> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public Task<ServiceResult<DashboardSummaryResponse>> GetSummaryAsync()
    {
        try
        {
            var (workers, alarms, sos) = _db.GetDashboardCounts(AlertWindowHours);

            return Task.FromResult(ServiceResult<DashboardSummaryResponse>.Ok(new DashboardSummaryResponse
            {
                TotalWorkers      = workers,
                ActiveAlerts      = alarms,
                SosCount          = sos,
                WorkersInDistress = sos
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSummaryAsync failed");
            return Task.FromResult(
                ServiceResult<DashboardSummaryResponse>.Fail("An unexpected error occurred.", 500));
        }
    }
}
