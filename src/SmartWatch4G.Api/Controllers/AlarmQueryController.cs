using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

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
    private readonly IAlarmRepository _alarmRepo;
    private readonly ILogger<AlarmQueryController> _logger;

    public AlarmQueryController(
        IAlarmRepository alarmRepo,
        ILogger<AlarmQueryController> logger)
    {
        _alarmRepo = alarmRepo;
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
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest(new ApiListResponse<AlarmEventDto> { ReturnCode = 400 });
        }

        IReadOnlyList<AlarmEventRecord> records;
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
                records = await _alarmRepo.GetByDeviceAndTimeRangeAsync(deviceId, from, to, ct)
                    .ConfigureAwait(false);
            }
            else if (DateTimeUtilities.IsValidDate(date))
            {
                filterDesc = $"date {date}";
                records = await _alarmRepo.GetByDeviceAndDateAsync(deviceId, date!, ct)
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

        var data = records.Select(r => MapToDto(r, tzInfo)).ToList();

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
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest(new ApiItemResponse<AlarmEventDto> { ReturnCode = 400 });
        }

        AlarmEventRecord? record;
        try
        {
            record = await _alarmRepo.GetLatestByDeviceAsync(deviceId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetAlarmLatest — DB read failed for device {DeviceId}", deviceId);
            return StatusCode(500, new ApiItemResponse<AlarmEventDto> { ReturnCode = 500 });
        }

        if (record is null)
        {
            _logger.LogInformation("GetAlarmLatest — no data for device {DeviceId}", deviceId);
            return NotFound(new ApiItemResponse<AlarmEventDto> { ReturnCode = 404 });
        }

        _logger.LogInformation(
            "GetAlarmLatest — exit, device: {DeviceId}, alarmType: {AlarmType}",
            deviceId, record.AlarmType);

        return Ok(new ApiItemResponse<AlarmEventDto>
        {
            ReturnCode = 0,
            Data = MapToDto(record, tzInfo)
        });
    }

    private static AlarmEventDto MapToDto(AlarmEventRecord r, TimeZoneInfo? tz) => new()
    {
        DeviceId  = r.DeviceId ?? string.Empty,
        AlarmType = r.AlarmType,
        AlarmTime = DateTimeUtilities.LocalizeTimestamp(r.AlarmTime, tz),
        Value1    = r.Value1,
        Value2    = r.Value2,
        Notes     = r.Notes,
        ReceivedAt = DateTimeUtilities.LocalizeDateTime(r.ReceivedAt, tz)
    };
}
