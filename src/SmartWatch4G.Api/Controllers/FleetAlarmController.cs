using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Fleet alarm endpoints consumed by mobile and web applications.
/// Routes:
///   GET /api/fleet/alarms?date=    — all alarm events across all devices on a date
///   GET /api/fleet/alarms/latest   — most recent alarm event per device
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("app-read")]
[ApiController]
[Route("api/v{version:apiVersion}/fleet")]
public sealed class FleetAlarmController : ControllerBase
{
    private readonly IAlarmRepository _alarmRepo;
    private readonly ILogger<FleetAlarmController> _logger;

    public FleetAlarmController(
        IAlarmRepository alarmRepo,
        ILogger<FleetAlarmController> logger)
    {
        _alarmRepo = alarmRepo;
        _logger = logger;
    }

    /// <summary>Returns all alarm events across all devices on the given date.</summary>
    [HttpGet("alarms")]
    public async Task<IActionResult> GetFleetAlarmsAsync(
        [FromQuery] string date,
        [FromQuery] string? tz,
        CancellationToken ct)
    {
        _logger.LogInformation("GetFleetAlarms — entry, date: {Date}", date);
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        if (!DateTimeUtilities.IsValidDate(date))
        {
            _logger.LogWarning("GetFleetAlarms — invalid date: {Date}", date);
            return BadRequest(new ApiListResponse<AlarmEventDto> { ReturnCode = 400 });
        }

        IReadOnlyList<AlarmEventRecord> records;
        try
        {
            records = await _alarmRepo.GetAllDevicesAndDateAsync(date, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFleetAlarms — DB read failed for date {Date}", date);
            return StatusCode(500, new ApiListResponse<AlarmEventDto> { ReturnCode = 500 });
        }

        var data = records.Select(r => MapToDto(r, tzInfo)).ToList();
        _logger.LogInformation("GetFleetAlarms — exit, date: {Date}, count: {Count}", date, data.Count);
        return Ok(new ApiListResponse<AlarmEventDto> { ReturnCode = 0, Count = data.Count, Data = data });
    }

    /// <summary>Returns the most recent alarm event for every device that has alarm history.</summary>
    [HttpGet("alarms/latest")]
    public async Task<IActionResult> GetFleetAlarmsLatestAsync([FromQuery] string? tz, CancellationToken ct)
    {
        _logger.LogInformation("GetFleetAlarmsLatest — entry");
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        IReadOnlyList<AlarmEventRecord> records;
        try
        {
            records = await _alarmRepo.GetLatestAllDevicesAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFleetAlarmsLatest — DB read failed");
            return StatusCode(500, new ApiListResponse<AlarmEventDto> { ReturnCode = 500 });
        }

        var data = records.Select(r => MapToDto(r, tzInfo)).ToList();
        _logger.LogInformation("GetFleetAlarmsLatest — exit, {Count} devices", data.Count);
        return Ok(new ApiListResponse<AlarmEventDto> { ReturnCode = 0, Count = data.Count, Data = data });
    }

    private static AlarmEventDto MapToDto(AlarmEventRecord r, TimeZoneInfo? tz) => new()
    {
        DeviceId   = r.DeviceId ?? string.Empty,
        AlarmType  = r.AlarmType,
        AlarmTime  = DateTimeUtilities.LocalizeTimestamp(r.AlarmTime, tz),
        Value1     = r.Value1,
        Value2     = r.Value2,
        Notes      = r.Notes,
        ReceivedAt = DateTimeUtilities.LocalizeDateTime(r.ReceivedAt, tz)
    };
}
