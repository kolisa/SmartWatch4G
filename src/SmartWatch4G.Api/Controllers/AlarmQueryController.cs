using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Application.Utilities;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Read-only alarm endpoints consumed by mobile and web applications.
/// Routes:
///   GET /api/devices/{deviceId}/alarms          — history (?date= or ?from=&amp;to=)
///   GET /api/devices/{deviceId}/alarms/latest   — most recent alarm event
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("app-read")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/alarms")]
public sealed class AlarmQueryController : ControllerBase
{
    private readonly IAlarmQueryService _alarmService;
    private readonly ILogger<AlarmQueryController> _logger;

    public AlarmQueryController(
        IAlarmQueryService alarmService,
        ILogger<AlarmQueryController> logger)
    {
        _alarmService = alarmService;
        _logger = logger;
    }

    /// <summary>
    /// Returns alarm events for a device.
    /// Supply either <c>?date=yyyy-MM-dd</c> or <c>?from=...&amp;to=...</c> (yyyy-MM-dd HH:mm:ss).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAlarmsAsync(
        string deviceId,
        [FromQuery] string? date,
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] string? tz,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "GetAlarms — entry, device: {DeviceId}, date: {Date}, from: {From}, to: {To}",
            deviceId, date, from, to);

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest(new ApiListResponse<AlarmEventDto> { ReturnCode = 400 });
        }

        IReadOnlyList<AlarmEventDto> data;
        string filterDesc;

        try
        {
            if (!string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(to))
            {
                if (!DateTimeUtilities.IsValidDateTime(from) || !DateTimeUtilities.IsValidDateTime(to))
                {
                    _logger.LogWarning(
                        "GetAlarms — invalid datetime range, from: {From}, to: {To}", from, to);
                    return BadRequest(new ApiListResponse<AlarmEventDto> { ReturnCode = 400 });
                }

                filterDesc = $"{from} → {to}";
                data = await _alarmService.GetByRangeAsync(deviceId, from, to, tz, ct)
                    .ConfigureAwait(false);
            }
            else if (DateTimeUtilities.IsValidDate(date))
            {
                filterDesc = $"date {date}";
                data = await _alarmService.GetByDateAsync(deviceId, date!, tz, ct)
                    .ConfigureAwait(false);
            }
            else
            {
                _logger.LogWarning("GetAlarms — no valid filter for device {DeviceId}", deviceId);
                return BadRequest(new ApiListResponse<AlarmEventDto> { ReturnCode = 400 });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAlarms — DB read failed for device {DeviceId}", deviceId);
            return StatusCode(500, new ApiListResponse<AlarmEventDto> { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "GetAlarms — exit, device: {DeviceId}, filter: [{Filter}], count: {Count}",
            deviceId, filterDesc, data.Count);

        return Ok(new ApiListResponse<AlarmEventDto>
        {
            ReturnCode = 0,
            Count = data.Count,
            Data = data
        });
    }

    /// <summary>Returns the single most recent alarm event for a device.</summary>
    [HttpGet("latest")]
    public async Task<IActionResult> GetAlarmLatestAsync(string deviceId, [FromQuery] string? tz, CancellationToken ct)
    {
        _logger.LogInformation("GetAlarmLatest — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest(new ApiItemResponse<AlarmEventDto> { ReturnCode = 400 });
        }

        AlarmEventDto? item;
        try
        {
            item = await _alarmService.GetLatestAsync(deviceId, tz, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetAlarmLatest — DB read failed for device {DeviceId}", deviceId);
            return StatusCode(500, new ApiItemResponse<AlarmEventDto> { ReturnCode = 500 });
        }

        if (item is null)
        {
            _logger.LogInformation("GetAlarmLatest — no data for device {DeviceId}", deviceId);
            return NotFound(new ApiItemResponse<AlarmEventDto> { ReturnCode = 404 });
        }

        _logger.LogInformation(
            "GetAlarmLatest — exit, device: {DeviceId}, alarmType: {AlarmType}",
            deviceId, item.AlarmType);

        return Ok(new ApiItemResponse<AlarmEventDto>
        {
            ReturnCode = 0,
            Data = item
        });
    }
}
