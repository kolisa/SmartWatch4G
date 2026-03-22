using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Read-only health-snapshot endpoints (1-minute aggregated metrics).
/// Supports date-only or explicit datetime range filtering.
///
/// Routes:
///   GET /api/devices/{deviceId}/health          — history (?date= or ?from=&amp;to=)
///   GET /api/devices/{deviceId}/health/latest   — most-recent snapshot
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("app-read")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/health")]
public sealed class HealthSnapshotController : ControllerBase
{
    private readonly IHealthDataRepository _healthRepo;
    private readonly ILogger<HealthSnapshotController> _logger;

    public HealthSnapshotController(
        IHealthDataRepository healthRepo,
        ILogger<HealthSnapshotController> logger)
    {
        _healthRepo = healthRepo;
        _logger = logger;
    }

    /// <summary>
    /// Returns 1-minute health snapshots for a device.
    /// Supply either <c>?date=yyyy-MM-dd</c> or <c>?from=...&amp;to=...</c> (yyyy-MM-dd HH:mm:ss).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetHealthAsync(
        string deviceId,
        [FromQuery] string? date,
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] string? tz,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "GetHealth — entry, device: {DeviceId}, date: {Date}, from: {From}, to: {To}",
            deviceId, date, from, to);
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest(new ApiListResponse<HealthSnapshotDto> { ReturnCode = 400 });
        }

        IReadOnlyList<HealthDataRecord> records;
        string filterDesc;

        try
        {
            if (!string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(to))
            {
                if (!DateTimeUtilities.IsValidDateTime(from) || !DateTimeUtilities.IsValidDateTime(to))
                {
                    _logger.LogWarning(
                        "GetHealth — invalid datetime range, from: {From}, to: {To}", from, to);
                    return BadRequest(new ApiListResponse<HealthSnapshotDto> { ReturnCode = 400 });
                }

                filterDesc = $"{from} → {to}";
                records = await _healthRepo.GetByDeviceAndTimeRangeAsync(deviceId, from, to, ct)
                    .ConfigureAwait(false);
            }
            else if (DateTimeUtilities.IsValidDate(date))
            {
                filterDesc = $"date {date}";
                records = await _healthRepo.GetByDeviceAndDateAsync(deviceId, date!, ct)
                    .ConfigureAwait(false);
            }
            else
            {
                _logger.LogWarning(
                    "GetHealth — no valid filter for device {DeviceId}", deviceId);
                return BadRequest(new ApiListResponse<HealthSnapshotDto> { ReturnCode = 400 });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetHealth — DB read failed for device {DeviceId}", deviceId);
            return StatusCode(500, new ApiListResponse<HealthSnapshotDto> { ReturnCode = 500 });
        }

        var data = records.Select(r => MapToDto(r, tzInfo)).ToList();

        _logger.LogInformation(
            "GetHealth — exit, device: {DeviceId}, filter: [{Filter}], count: {Count}",
            deviceId, filterDesc, data.Count);

        return Ok(new ApiListResponse<HealthSnapshotDto>
        {
            ReturnCode = 0,
            Count = data.Count,
            Data = data
        });
    }

    /// <summary>Returns the single most recent health snapshot for a device.</summary>
    [HttpGet("latest")]
    public async Task<IActionResult> GetHealthLatestAsync(string deviceId, [FromQuery] string? tz, CancellationToken ct)
    {
        _logger.LogInformation("GetHealthLatest — entry, device: {DeviceId}", deviceId);
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest(new ApiItemResponse<HealthSnapshotDto> { ReturnCode = 400 });
        }

        HealthDataRecord? record;
        try
        {
            record = await _healthRepo.GetLatestByDeviceAsync(deviceId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetHealthLatest — DB read failed for device {DeviceId}", deviceId);
            return StatusCode(500, new ApiItemResponse<HealthSnapshotDto> { ReturnCode = 500 });
        }

        if (record is null)
        {
            _logger.LogInformation("GetHealthLatest — no data for device {DeviceId}", deviceId);
            return NotFound(new ApiItemResponse<HealthSnapshotDto> { ReturnCode = 404 });
        }

        _logger.LogInformation(
            "GetHealthLatest — exit, device: {DeviceId}, dataTime: {DataTime}",
            deviceId, record.DataTime);

        return Ok(new ApiItemResponse<HealthSnapshotDto>
        {
            ReturnCode = 0,
            Data = MapToDto(record, tzInfo)
        });
    }

    private static HealthSnapshotDto MapToDto(HealthDataRecord r, TimeZoneInfo? tz) => new()
    {
        DeviceId = r.DeviceId ?? string.Empty,
        DataTime = DateTimeUtilities.LocalizeTimestamp(r.DataTime, tz),
        Steps = r.Steps,
        DistanceMetres = r.DistanceMetres,
        CaloriesKcal = r.CaloriesKcal,
        ActivityType = r.ActivityType,
        ActivityState = r.ActivityState,
        AvgHeartRate = r.AvgHeartRate,
        MaxHeartRate = r.MaxHeartRate,
        MinHeartRate = r.MinHeartRate,
        AvgSpo2 = r.AvgSpo2,
        Sbp = r.Sbp,
        Dbp = r.Dbp,
        HrvSdnn = r.HrvSdnn,
        HrvRmssd = r.HrvRmssd,
        HrvPnn50 = r.HrvPnn50,
        HrvMean = r.HrvMean,
        Fatigue = r.Fatigue,
        AxillaryTemp = r.AxillaryTemp,
        EstimatedTemp = r.EstimatedTemp,
        BodyFat = r.BodyFat,
        Bmi = r.Bmi,
        BloodSugar = r.BloodSugar,
        BloodPotassium = r.BloodPotassium
    };
}
