using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Fleet health endpoints consumed by mobile and web applications.
/// Routes:
///   GET /api/fleet/health/latest        — most-recent health snapshot per device
///   GET /api/fleet/health/summary?date= — aggregated daily health stats per device
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("dashboard-api")]
[ApiController]
[Route("api/v{version:apiVersion}/fleet")]
public sealed class FleetHealthController : ControllerBase
{
    private readonly IHealthQueryService _healthService;
    private readonly ILogger<FleetHealthController> _logger;
    private readonly IDateTimeService _dt;

    public FleetHealthController(
        IHealthQueryService healthService,
        ILogger<FleetHealthController> logger,
        IDateTimeService dt)
    {
        _healthService = healthService;
        _logger = logger;
        _dt = dt;
    }

    /// <summary>Returns the most recent health snapshot for every device.</summary>
    [HttpGet("health/latest")]
    public async Task<IActionResult> GetFleetHealthLatestAsync([FromQuery] string? tz, CancellationToken ct)
    {
        _logger.LogInformation("GetFleetHealthLatest — entry");

        IReadOnlyList<HealthSnapshotDto> data;
        try
        {
            data = await _healthService.GetLatestSnapshotAllDevicesAsync(tz, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFleetHealthLatest — DB read failed");
            return StatusCode(500, new ApiListResponse<HealthSnapshotDto> { ReturnCode = 500 });
        }

        _logger.LogInformation("GetFleetHealthLatest — exit, {Count} devices", data.Count);
        return Ok(new ApiListResponse<HealthSnapshotDto> { ReturnCode = 0, Count = data.Count, Data = data });
    }

    /// <summary>
    /// Returns aggregated daily health statistics (min/max/avg HR, avg SpO2, total steps, etc.)
    /// for every device that has data on the given date.
    /// </summary>
    [HttpGet("health/summary")]
    public async Task<IActionResult> GetFleetHealthSummaryAsync(
        [FromQuery] string date,
        CancellationToken ct)
    {
        _logger.LogInformation("GetFleetHealthSummary — entry, date: {Date}", date);

        if (!_dt.IsValidDate(date))
        {
            _logger.LogWarning("GetFleetHealthSummary — invalid date: {Date}", date);
            return BadRequest(new ApiListResponse<HealthDailyStatsDto> { ReturnCode = 400 });
        }

        IReadOnlyList<HealthDailyStatsDto> data;
        try
        {
            data = await _healthService.GetDailyStatsAllDevicesAsync(date, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFleetHealthSummary — DB read failed for date {Date}", date);
            return StatusCode(500, new ApiListResponse<HealthDailyStatsDto> { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "GetFleetHealthSummary — exit, date: {Date}, devices: {Count}", date, data.Count);

        return Ok(new ApiListResponse<HealthDailyStatsDto>
        {
            ReturnCode = 0,
            Count = data.Count,
            Data = data
        });
    }
}
