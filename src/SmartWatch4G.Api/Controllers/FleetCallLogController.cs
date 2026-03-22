using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Fleet call-log endpoint consumed by mobile and web applications.
/// Route: GET /api/fleet/call-logs?date=
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("app-read")]
[ApiController]
[Route("api/v{version:apiVersion}/fleet")]
public sealed class FleetCallLogController : ControllerBase
{
    private readonly ICallLogRepository _callLogRepo;
    private readonly ILogger<FleetCallLogController> _logger;

    public FleetCallLogController(
        ICallLogRepository callLogRepo,
        ILogger<FleetCallLogController> logger)
    {
        _callLogRepo = callLogRepo;
        _logger = logger;
    }

    /// <summary>Returns all call-log entries across all devices on the given date.</summary>
    [HttpGet("call-logs")]
    public async Task<IActionResult> GetFleetCallLogsAsync(
        [FromQuery] string date,
        [FromQuery] string? tz,
        CancellationToken ct)
    {
        _logger.LogInformation("GetFleetCallLogs — entry, date: {Date}", date);
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        if (!DateTimeUtilities.IsValidDate(date))
        {
            _logger.LogWarning("GetFleetCallLogs — invalid date: {Date}", date);
            return BadRequest(new ApiListResponse<CallLogItemDto> { ReturnCode = 400 });
        }

        IReadOnlyList<CallLogRecord> records;
        try
        {
            records = await _callLogRepo.GetAllDevicesAndDateAsync(date, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFleetCallLogs — DB read failed for date {Date}", date);
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
            "GetFleetCallLogs — exit, date: {Date}, count: {Count}", date, data.Count);

        return Ok(new ApiListResponse<CallLogItemDto>
        {
            ReturnCode = 0,
            Count = data.Count,
            Data = data
        });
    }
}
