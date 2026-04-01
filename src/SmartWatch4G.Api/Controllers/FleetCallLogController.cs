using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Application.Utilities;

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
    private readonly ICallLogQueryService _callLogService;
    private readonly ILogger<FleetCallLogController> _logger;

    public FleetCallLogController(
        ICallLogQueryService callLogService,
        ILogger<FleetCallLogController> logger)
    {
        _callLogService = callLogService;
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

        if (!DateTimeUtilities.IsValidDate(date))
        {
            _logger.LogWarning("GetFleetCallLogs — invalid date: {Date}", date);
            return BadRequest(new ApiListResponse<CallLogItemDto> { ReturnCode = 400 });
        }

        IReadOnlyList<CallLogItemDto> data;
        try
        {
            data = await _callLogService.GetAllDevicesAndDateAsync(date, tz, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFleetCallLogs — DB read failed for date {Date}", date);
            return StatusCode(500, new ApiListResponse<CallLogItemDto> { ReturnCode = 500 });
        }

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
