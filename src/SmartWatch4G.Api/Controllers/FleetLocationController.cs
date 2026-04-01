using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Application.Utilities;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Fleet GPS / location endpoints consumed by mobile and web applications.
/// Routes:
///   GET /api/fleet/location/latest         — most-recent GPS point per device
///   GET /api/fleet/location?date=          — full GPS track for all devices on a date
///   GET /api/fleet/location/recent?minutes=— all devices' locations in the last N minutes
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("app-read")]
[ApiController]
[Route("api/v{version:apiVersion}/fleet")]
public sealed class FleetLocationController : ControllerBase
{
    private readonly ILocationQueryService _locationService;
    private readonly ILogger<FleetLocationController> _logger;

    public FleetLocationController(
        ILocationQueryService locationService,
        ILogger<FleetLocationController> logger)
    {
        _locationService = locationService;
        _logger = logger;
    }

    /// <summary>Returns the most recent GPS point for every device. Useful for a live fleet map.</summary>
    [HttpGet("location/latest")]
    public async Task<IActionResult> GetFleetLocationLatestAsync([FromQuery] string? tz, CancellationToken ct)
    {
        _logger.LogInformation("GetFleetLocationLatest — entry");

        IReadOnlyList<LocationPointDto> data;
        try
        {
            data = await _locationService.GetLatestAllDevicesAsync(tz, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFleetLocationLatest — DB read failed");
            return StatusCode(500, new ApiListResponse<LocationPointDto> { ReturnCode = 500 });
        }

        _logger.LogInformation("GetFleetLocationLatest — exit, {Count} devices", data.Count);
        return Ok(new ApiListResponse<LocationPointDto> { ReturnCode = 0, Count = data.Count, Data = data });
    }

    /// <summary>Returns all GPS track points for all devices on the given date.</summary>
    [HttpGet("location")]
    public async Task<IActionResult> GetFleetLocationAsync(
        [FromQuery] string date,
        [FromQuery] string? tz,
        CancellationToken ct)
    {
        _logger.LogInformation("GetFleetLocation — entry, date: {Date}", date);

        if (!DateTimeUtilities.IsValidDate(date))
        {
            _logger.LogWarning("GetFleetLocation — invalid date: {Date}", date);
            return BadRequest(new ApiListResponse<LocationPointDto> { ReturnCode = 400 });
        }

        IReadOnlyList<LocationPointDto> data;
        try
        {
            data = await _locationService.GetAllDevicesAndDateAsync(date, tz, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFleetLocation — DB read failed for date {Date}", date);
            return StatusCode(500, new ApiListResponse<LocationPointDto> { ReturnCode = 500 });
        }

        _logger.LogInformation("GetFleetLocation — exit, date: {Date}, count: {Count}", date, data.Count);
        return Ok(new ApiListResponse<LocationPointDto> { ReturnCode = 0, Count = data.Count, Data = data });
    }

}
