using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Application.Utilities;

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
[EnableRateLimiting("dashboard-api")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/health")]
public sealed class HealthSnapshotController : ControllerBase
{
    private readonly IHealthQueryService _healthService;
    private readonly ILogger<HealthSnapshotController> _logger;

    public HealthSnapshotController(
        IHealthQueryService healthService,
        ILogger<HealthSnapshotController> logger)
    {
        _healthService = healthService;
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

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest(new ApiListResponse<HealthSnapshotDto> { ReturnCode = 400 });
        }

        IReadOnlyList<HealthSnapshotDto> data;
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
                data = await _healthService.GetSnapshotsByRangeAsync(deviceId, from, to, tz, ct)
                    .ConfigureAwait(false);
            }
            else if (DateTimeUtilities.IsValidDate(date))
            {
                filterDesc = $"date {date}";
                data = await _healthService.GetSnapshotsByDateAsync(deviceId, date!, tz, ct)
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

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest(new ApiItemResponse<HealthSnapshotDto> { ReturnCode = 400 });
        }

        HealthSnapshotDto? item;
        try
        {
            item = await _healthService.GetLatestSnapshotAsync(deviceId, tz, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetHealthLatest — DB read failed for device {DeviceId}", deviceId);
            return StatusCode(500, new ApiItemResponse<HealthSnapshotDto> { ReturnCode = 500 });
        }

        if (item is null)
        {
            _logger.LogInformation("GetHealthLatest — no data for device {DeviceId}", deviceId);
            return NotFound(new ApiItemResponse<HealthSnapshotDto> { ReturnCode = 404 });
        }

        _logger.LogInformation(
            "GetHealthLatest — exit, device: {DeviceId}, dataTime: {DataTime}",
            deviceId, item.DataTime);

        return Ok(new ApiItemResponse<HealthSnapshotDto>
        {
            ReturnCode = 0,
            Data = item
        });
    }
}
