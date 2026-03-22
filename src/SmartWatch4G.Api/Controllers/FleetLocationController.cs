using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

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
    private readonly IGnssTrackRepository _gnssRepo;
    private readonly ILogger<FleetLocationController> _logger;

    public FleetLocationController(
        IGnssTrackRepository gnssRepo,
        ILogger<FleetLocationController> logger)
    {
        _gnssRepo = gnssRepo;
        _logger = logger;
    }

    /// <summary>Returns the most recent GPS point for every device. Useful for a live fleet map.</summary>
    [HttpGet("location/latest")]
    public async Task<IActionResult> GetFleetLocationLatestAsync([FromQuery] string? tz, CancellationToken ct)
    {
        _logger.LogInformation("GetFleetLocationLatest — entry");
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        IReadOnlyList<GnssTrackRecord> records;
        try
        {
            records = await _gnssRepo.GetLatestAllDevicesAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFleetLocationLatest — DB read failed");
            return StatusCode(500, new ApiListResponse<LocationPointDto> { ReturnCode = 500 });
        }

        var data = records.Select(r => MapToDto(r, tzInfo)).ToList();
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
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        if (!DateTimeUtilities.IsValidDate(date))
        {
            _logger.LogWarning("GetFleetLocation — invalid date: {Date}", date);
            return BadRequest(new ApiListResponse<LocationPointDto> { ReturnCode = 400 });
        }

        IReadOnlyList<GnssTrackRecord> records;
        try
        {
            records = await _gnssRepo.GetAllDevicesAndDateAsync(date, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFleetLocation — DB read failed for date {Date}", date);
            return StatusCode(500, new ApiListResponse<LocationPointDto> { ReturnCode = 500 });
        }

        var data = records.Select(r => MapToDto(r, tzInfo)).ToList();
        _logger.LogInformation("GetFleetLocation — exit, date: {Date}, count: {Count}", date, data.Count);
        return Ok(new ApiListResponse<LocationPointDto> { ReturnCode = 0, Count = data.Count, Data = data });
    }

    /// <summary>
    /// Returns all devices' GPS track points from the last N minutes.
    /// Useful for real-time fleet proximity and geofence checks.
    /// </summary>
    [HttpGet("location/recent")]
    public async Task<IActionResult> GetFleetLocationRecentAsync(
        [FromQuery] int minutes,
        [FromQuery] string? tz,
        CancellationToken ct)
    {
        _logger.LogInformation("GetFleetLocationRecent — entry, minutes: {Minutes}", minutes);
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        if (minutes <= 0)
        {
            _logger.LogWarning("GetFleetLocationRecent — invalid minutes: {Minutes}", minutes);
            return BadRequest(new ApiListResponse<LocationPointDto> { ReturnCode = 400 });
        }

        // Collect recent tracks for all devices in parallel via GetAllDevicesAndDateAsync
        // is not suitable here; instead use today's date and filter by time in the repo.
        // We reuse the single-device GetRecentByDeviceAsync across devices via a fleet-level
        // query: get today's full fleet track then filter client-side, or add a new repo method.
        // For correctness we call the fleet date query spanning today (UTC) only, but the
        // GnssTrackRepository.GetRecentByDeviceAsync already handles the UTC window — we expose
        // a fleet variant via a new overload in the repo interface (GetRecentAllDevicesAsync).
        // Until that is available we compute the window here and reuse GetAllDevicesAndDateAsync
        // with an additional in-memory time filter.
        System.DateTime now = System.DateTime.UtcNow;
        string fromStr = now.AddMinutes(-minutes)
            .ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        string toStr = now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        string date = now.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

        IReadOnlyList<GnssTrackRecord> allToday;
        try
        {
            allToday = await _gnssRepo.GetAllDevicesAndDateAsync(date, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFleetLocationRecent — DB read failed");
            return StatusCode(500, new ApiListResponse<LocationPointDto> { ReturnCode = 500 });
        }

        var data = allToday
            .Where(r => string.Compare(r.TrackTime, fromStr, StringComparison.Ordinal) >= 0
                     && string.Compare(r.TrackTime, toStr,   StringComparison.Ordinal) <= 0)
            .Select(r => MapToDto(r, tzInfo))
            .ToList();

        _logger.LogInformation(
            "GetFleetLocationRecent — exit, minutes: {Minutes}, count: {Count}", minutes, data.Count);

        return Ok(new ApiListResponse<LocationPointDto> { ReturnCode = 0, Count = data.Count, Data = data });
    }

    private static LocationPointDto MapToDto(GnssTrackRecord r, TimeZoneInfo? tz) => new()
    {
        DeviceId      = r.DeviceId ?? string.Empty,
        TrackTime     = DateTimeUtilities.LocalizeTimestamp(r.TrackTime, tz),
        Longitude     = r.Longitude,
        Latitude      = r.Latitude,
        GpsType       = r.GpsType,
        BatteryLevel  = r.BatteryLevel,
        Rssi          = r.Rssi,
        Steps         = r.Steps,
        DistanceMetres = r.DistanceMetres,
        CaloriesKcal  = r.CaloriesKcal
    };
}
