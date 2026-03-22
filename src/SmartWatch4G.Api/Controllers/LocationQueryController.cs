using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

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
[EnableRateLimiting("app-read")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/location")]
public sealed class LocationQueryController : ControllerBase
{
    private readonly IGnssTrackRepository _gnssRepo;
    private readonly ILogger<LocationQueryController> _logger;

    public LocationQueryController(
        IGnssTrackRepository gnssRepo,
        ILogger<LocationQueryController> logger)
    {
        _gnssRepo = gnssRepo;
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
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest(new ApiListResponse<LocationPointDto> { ReturnCode = 400 });
        }

        IReadOnlyList<GnssTrackRecord> records;
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
                records = await _gnssRepo.GetRecentByDeviceAsync(deviceId, minutes.Value, ct)
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
                records = await _gnssRepo.GetByDeviceAndTimeRangeAsync(deviceId, from, to, ct)
                    .ConfigureAwait(false);
            }
            else if (DateTimeUtilities.IsValidDate(date))
            {
                filterDesc = $"date {date}";
                records = await _gnssRepo.GetByDeviceAndDateAsync(deviceId, date!, ct)
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

        var data = records.Select(r => MapToDto(r, tzInfo)).ToList();

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
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest(new ApiItemResponse<LocationPointDto> { ReturnCode = 400 });
        }

        GnssTrackRecord? record;
        try
        {
            record = await _gnssRepo.GetLatestByDeviceAsync(deviceId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetLocationLatest — DB read failed for device {DeviceId}", deviceId);
            return StatusCode(500, new ApiItemResponse<LocationPointDto> { ReturnCode = 500 });
        }

        if (record is null)
        {
            _logger.LogInformation("GetLocationLatest — no data for device {DeviceId}", deviceId);
            return NotFound(new ApiItemResponse<LocationPointDto> { ReturnCode = 404 });
        }

        _logger.LogInformation(
            "GetLocationLatest — exit, device: {DeviceId}, trackTime: {TrackTime}",
            deviceId, record.TrackTime);

        return Ok(new ApiItemResponse<LocationPointDto> { ReturnCode = 0, Data = MapToDto(record, tzInfo) });
    }

    private static LocationPointDto MapToDto(GnssTrackRecord r, TimeZoneInfo? tz) => new()
    {
        DeviceId = r.DeviceId ?? string.Empty,
        TrackTime = DateTimeUtilities.LocalizeTimestamp(r.TrackTime, tz),
        Longitude = r.Longitude,
        Latitude = r.Latitude,
        GpsType = r.GpsType,
        BatteryLevel = r.BatteryLevel,
        Rssi = r.Rssi,
        Steps = r.Steps,
        DistanceMetres = r.DistanceMetres,
        CaloriesKcal = r.CaloriesKcal
    };
}
