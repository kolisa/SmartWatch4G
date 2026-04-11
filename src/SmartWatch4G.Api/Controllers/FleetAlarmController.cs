using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Fleet alarm endpoints consumed by mobile and web applications.
/// Routes:
///   GET /api/fleet/alarms?date=    — all alarm events across all devices on a date
///   GET /api/fleet/alarms/latest   — most recent alarm event per device
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("dashboard-api")]
[ApiController]
[Route("api/v{version:apiVersion}/fleet")]
public sealed class FleetAlarmController : ControllerBase
{
    private readonly IAlarmQueryService _alarmService;
    private readonly ILogger<FleetAlarmController> _logger;
    private readonly IDateTimeService _dt;

    public FleetAlarmController(
        IAlarmQueryService alarmService,
        ILogger<FleetAlarmController> logger,
        IDateTimeService dt)
    {
        _alarmService = alarmService;
        _logger = logger;
        _dt = dt;
    }

    /// <summary>Returns all alarm events across all devices on the given date.</summary>
    [HttpGet("alarms")]
    public async Task<IActionResult> GetFleetAlarmsAsync(
        [FromQuery] string date,
        [FromQuery] string? tz,
        CancellationToken ct)
    {
        _logger.LogInformation("GetFleetAlarms — entry, date: {Date}", date);

        if (!_dt.IsValidDate(date))
        {
            _logger.LogWarning("GetFleetAlarms — invalid date: {Date}", date);
            return BadRequest(new ApiListResponse<AlarmEventDto> { ReturnCode = 400 });
        }

        IReadOnlyList<AlarmEventDto> data;
        try
        {
            data = await _alarmService.GetAllDevicesAndDateAsync(date, tz, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFleetAlarms — DB read failed for date {Date}", date);
            return StatusCode(500, new ApiListResponse<AlarmEventDto> { ReturnCode = 500 });
        }

        _logger.LogInformation("GetFleetAlarms — exit, date: {Date}, count: {Count}", date, data.Count);
        return Ok(new ApiListResponse<AlarmEventDto> { ReturnCode = 0, Count = data.Count, Data = data });
    }

    /// <summary>Returns the most recent alarm event for every device that has alarm history.</summary>
    [HttpGet("alarms/latest")]
    public async Task<IActionResult> GetFleetAlarmsLatestAsync([FromQuery] string? tz, CancellationToken ct)
    {
        _logger.LogInformation("GetFleetAlarmsLatest — entry");

        IReadOnlyList<AlarmEventDto> data;
        try
        {
            data = await _alarmService.GetLatestAllDevicesAsync(tz, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFleetAlarmsLatest — DB read failed");
            return StatusCode(500, new ApiListResponse<AlarmEventDto> { ReturnCode = 500 });
        }

        _logger.LogInformation("GetFleetAlarmsLatest — exit, {Count} devices", data.Count);
        return Ok(new ApiListResponse<AlarmEventDto> { ReturnCode = 0, Count = data.Count, Data = data });
    }
}
