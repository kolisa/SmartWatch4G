using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Read-only call-log endpoints consumed by mobile and web applications.
/// Route: GET /api/devices/{deviceId}/call-logs
/// Supports filtering by <c>?date=yyyy-MM-dd</c> or <c>?from=...&amp;to=...</c>.
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("dashboard-api")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/call-logs")]
public sealed class CallLogQueryController : ControllerBase
{
    private readonly ICallLogQueryService _callLogService;
    private readonly ILogger<CallLogQueryController> _logger;
    private readonly IDateTimeService _dt;

    public CallLogQueryController(
        ICallLogQueryService callLogService,
        ILogger<CallLogQueryController> logger,
        IDateTimeService dt)
    {
        _callLogService = callLogService;
        _logger = logger;
        _dt = dt;
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

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest(new ApiListResponse<CallLogItemDto> { ReturnCode = 400 });
        }

        IReadOnlyList<CallLogItemDto> data;
        string filterDesc;

        try
        {
            if (!string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(to))
            {
                if (!_dt.IsValidDateTime(from) || !_dt.IsValidDateTime(to))
                {
                    _logger.LogWarning(
                        "GetCallLogs — invalid datetime range, from: {From}, to: {To}", from, to);
                    return BadRequest(new ApiListResponse<CallLogItemDto> { ReturnCode = 400 });
                }

                filterDesc = $"{from} → {to}";
                data = await _callLogService.GetByRangeAsync(deviceId, from, to, tz, ct)
                    .ConfigureAwait(false);
            }
            else if (_dt.IsValidDate(date))
            {
                filterDesc = $"date {date}";
                data = await _callLogService.GetByDateAsync(deviceId, date!, tz, ct)
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
