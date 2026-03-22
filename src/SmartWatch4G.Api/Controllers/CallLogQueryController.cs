using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Read-only call-log endpoints consumed by mobile and web applications.
/// Route: GET /api/devices/{deviceId}/call-logs
/// Supports filtering by <c>?date=yyyy-MM-dd</c> or <c>?from=...&amp;to=...</c>.
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("app-read")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/call-logs")]
public sealed class CallLogQueryController : ControllerBase
{
    private readonly ICallLogRepository _callLogRepo;
    private readonly ILogger<CallLogQueryController> _logger;

    public CallLogQueryController(
        ICallLogRepository callLogRepo,
        ILogger<CallLogQueryController> logger)
    {
        _callLogRepo = callLogRepo;
        _logger = logger;
    }

    /// <summary>
    /// Returns call-log entries for a device.
    /// Supply either <c>?date=yyyy-MM-dd</c> or <c>?from=...&amp;to=...</c> (yyyy-MM-dd HH:mm:ss).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCallLogsAsync(
        string deviceId,
        [FromQuery] string? date,
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] string? tz,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "GetCallLogs — entry, device: {DeviceId}, date: {Date}, from: {From}, to: {To}",
            deviceId, date, from, to);
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest(new ApiListResponse<CallLogItemDto> { ReturnCode = 400 });
        }

        IReadOnlyList<CallLogRecord> records;
        string filterDesc;

        try
        {
            if (!string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(to))
            {
                if (!DateTimeUtilities.IsValidDateTime(from) || !DateTimeUtilities.IsValidDateTime(to))
                {
                    _logger.LogWarning(
                        "GetCallLogs — invalid datetime range, from: {From}, to: {To}", from, to);
                    return BadRequest(new ApiListResponse<CallLogItemDto> { ReturnCode = 400 });
                }

                filterDesc = $"{from} → {to}";
                records = await _callLogRepo.GetByDeviceAndTimeRangeAsync(deviceId, from, to, ct)
                    .ConfigureAwait(false);
            }
            else if (DateTimeUtilities.IsValidDate(date))
            {
                filterDesc = $"date {date}";
                records = await _callLogRepo.GetByDeviceAndDateAsync(deviceId, date!, ct)
                    .ConfigureAwait(false);
            }
            else
            {
                _logger.LogWarning("GetCallLogs — no valid filter for device {DeviceId}", deviceId);
                return BadRequest(new ApiListResponse<CallLogItemDto> { ReturnCode = 400 });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCallLogs — DB read failed for device {DeviceId}", deviceId);
            return StatusCode(500, new ApiListResponse<CallLogItemDto> { ReturnCode = 500 });
        }

        var data = records.Select(r => new CallLogItemDto
        {
            DeviceId   = r.DeviceId,
            CallStatus = r.CallStatus,
            CallNumber = r.CallNumber,
            StartTime  = DateTimeUtilities.LocalizeTimestamp(r.StartTime, tzInfo),
            EndTime    = DateTimeUtilities.LocalizeTimestamp(r.EndTime, tzInfo),
            IsSosAlarm = r.IsSosAlarm,
            AlarmTime  = DateTimeUtilities.LocalizeTimestamp(r.AlarmTime, tzInfo),
            AlarmLat   = r.AlarmLat,
            AlarmLon   = r.AlarmLon,
            ReceivedAt = DateTimeUtilities.LocalizeDateTime(r.ReceivedAt, tzInfo)
        }).ToList();

        _logger.LogInformation(
            "GetCallLogs — exit, device: {DeviceId}, filter: [{Filter}], count: {Count}",
            deviceId, filterDesc, data.Count);

        return Ok(new ApiListResponse<CallLogItemDto>
        {
            ReturnCode = 0,
            Count = data.Count,
            Data = data
        });
    }
}
