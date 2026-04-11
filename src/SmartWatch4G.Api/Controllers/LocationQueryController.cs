using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Application.Utilities;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Read-only GPS/location endpoints consumed by mobile and web applications.
///
/// Filter priority for the history endpoint (first matching parameter wins):
///   1. <c>?minutes=N</c>           — points received in the last N minutes (real-time tracking)
///   2. <c>?from=...&amp;to=...</c> — explicit datetime range (yyyy-MM-dd HH:mm:ss)
///   3. <c>?date=yyyy-MM-dd</c>     — full calendar day
///
/// Routes:
///   GET /api/devices/{deviceId}/location          — GPS history
///   GET /api/devices/{deviceId}/location/latest   — most-recent GPS point
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("dashboard-api")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/location")]
public sealed class LocationQueryController : ControllerBase
{
    private readonly ILocationQueryService _locationService;
    private readonly ILogger<LocationQueryController> _logger;

    public LocationQueryController(
        ILocationQueryService locationService,
        ILogger<LocationQueryController> logger)
    {
        _locationService = locationService;
        _logger = logger;
    }

    /// <summary>
    /// Returns GPS track points for a device.
    /// Supply exactly one filter: <c>?minutes=N</c>, <c>?from=&amp;to=</c>, or <c>?date=</c>.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetLocationAsync(
        string deviceId,
        [FromQuery] string? date,
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] int? minutes,
        [FromQuery] string? tz,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "GetLocation — entry, device: {DeviceId}, minutes: {Minutes}, from: {From}, to: {To}, date: {Date}",
            deviceId, minutes, from, to, date);

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest(new ApiListResponse<LocationPointDto> { ReturnCode = 400 });
        }

        IReadOnlyList<LocationPointDto> data;
        string filterDesc;

        try
        {
            if (minutes.HasValue)
            {
                if (minutes.Value <= 0)
                {
                    return BadRequest(new ApiListResponse<LocationPointDto> { ReturnCode = 400 });
                }

                filterDesc = $"last {minutes.Value} min";
                data = await _locationService.GetRecentAsync(deviceId, minutes.Value, tz, ct)
                    .ConfigureAwait(false);
            }
            else if (!string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(to))
            {
                if (!DateTimeUtilities.IsValidDateTime(from) || !DateTimeUtilities.IsValidDateTime(to))
                {
                    _logger.LogWarning(
                        "GetLocation — invalid datetime range, from: {From}, to: {To}", from, to);
                    return BadRequest(new ApiListResponse<LocationPointDto> { ReturnCode = 400 });
                }

                filterDesc = $"{from} → {to}";
                data = await _locationService.GetByRangeAsync(deviceId, from, to, tz, ct)
                    .ConfigureAwait(false);
            }
            else if (DateTimeUtilities.IsValidDate(date))
            {
                filterDesc = $"date {date}";
                data = await _locationService.GetByDateAsync(deviceId, date!, tz, ct)
                    .ConfigureAwait(false);
            }
            else
            {
                _logger.LogWarning(
                    "GetLocation — no valid filter supplied for device {DeviceId}", deviceId);
                return BadRequest(new ApiListResponse<LocationPointDto> { ReturnCode = 400 });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetLocation — DB read failed for device {DeviceId}", deviceId);
            return StatusCode(500, new ApiListResponse<LocationPointDto> { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "GetLocation — exit, device: {DeviceId}, filter: [{Filter}], count: {Count}",
            deviceId, filterDesc, data.Count);

        return Ok(new ApiListResponse<LocationPointDto>
        {
            ReturnCode = 0,
            Count = data.Count,
            Data = data
        });
    }

    /// <summary>Returns the single most recent GPS point for a device.</summary>
    [HttpGet("latest")]
    public async Task<IActionResult> GetLocationLatestAsync(string deviceId, [FromQuery] string? tz, CancellationToken ct)
    {
        _logger.LogInformation("GetLocationLatest — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest(new ApiItemResponse<LocationPointDto> { ReturnCode = 400 });
        }

        LocationPointDto? item;
        try
        {
            item = await _locationService.GetLatestAsync(deviceId, tz, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetLocationLatest — DB read failed for device {DeviceId}", deviceId);
            return StatusCode(500, new ApiItemResponse<LocationPointDto> { ReturnCode = 500 });
        }

        if (item is null)
        {
            _logger.LogInformation("GetLocationLatest — no data for device {DeviceId}", deviceId);
            return NotFound(new ApiItemResponse<LocationPointDto> { ReturnCode = 404 });
        }

        _logger.LogInformation(
            "GetLocationLatest — exit, device: {DeviceId}, trackTime: {TrackTime}",
            deviceId, item.TrackTime);

        return Ok(new ApiItemResponse<LocationPointDto> { ReturnCode = 0, Data = item });
    }
}
