using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Read-only SpO2 (blood-oxygen) endpoints consumed by mobile and web applications.
/// Routes:
///   GET /api/devices/{deviceId}/spo2          — history (?date= or ?from=&amp;to=)
///   GET /api/devices/{deviceId}/spo2/latest   — most recent reading
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("dashboard-api")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/spo2")]
public sealed class Spo2QueryController : ControllerBase
{
    private readonly ISpo2QueryService _spo2Service;
    private readonly ILogger<Spo2QueryController> _logger;
    private readonly IDateTimeService _dt;

    public Spo2QueryController(
        ISpo2QueryService spo2Service,
        ILogger<Spo2QueryController> logger,
        IDateTimeService dt)
    {
        _spo2Service = spo2Service;
        _logger = logger;
        _dt = dt;
    }

    /// <summary>
    /// Returns SpO2 readings for a device.
    /// Supply either <c>?date=yyyy-MM-dd</c> or <c>?from=...&amp;to=...</c> (yyyy-MM-dd HH:mm:ss).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSpo2Async(
        string deviceId,
        [FromQuery] string? date,
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] string? tz,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "GetSpo2 — entry, device: {DeviceId}, date: {Date}, from: {From}, to: {To}",
            deviceId, date, from, to);

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest(new ApiListResponse<Spo2ReadingDto> { ReturnCode = 400 });
        }

        IReadOnlyList<Spo2ReadingDto> data;
        string filterDesc;

        try
        {
            if (!string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(to))
            {
                if (!_dt.IsValidDateTime(from) || !_dt.IsValidDateTime(to))
                {
                    _logger.LogWarning(
                        "GetSpo2 — invalid datetime range, from: {From}, to: {To}", from, to);
                    return BadRequest(new ApiListResponse<Spo2ReadingDto> { ReturnCode = 400 });
                }

                filterDesc = $"{from} → {to}";
                data = await _spo2Service.GetByRangeAsync(deviceId, from, to, tz, ct)
                    .ConfigureAwait(false);
            }
            else if (_dt.IsValidDate(date))
            {
                filterDesc = $"date {date}";
                data = await _spo2Service.GetByDateAsync(deviceId, date!, tz, ct)
                    .ConfigureAwait(false);
            }
            else
            {
                _logger.LogWarning("GetSpo2 — no valid filter for device {DeviceId}", deviceId);
                return BadRequest(new ApiListResponse<Spo2ReadingDto> { ReturnCode = 400 });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSpo2 — DB read failed for device {DeviceId}", deviceId);
            return StatusCode(500, new ApiListResponse<Spo2ReadingDto> { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "GetSpo2 — exit, device: {DeviceId}, filter: [{Filter}], count: {Count}",
            deviceId, filterDesc, data.Count);

        return Ok(new ApiListResponse<Spo2ReadingDto>
        {
            ReturnCode = 0,
            Count = data.Count,
            Data = data
        });
    }

    /// <summary>Returns the single most recent SpO2 reading for a device.</summary>
    [HttpGet("latest")]
    public async Task<IActionResult> GetSpo2LatestAsync(string deviceId, [FromQuery] string? tz, CancellationToken ct)
    {
        _logger.LogInformation("GetSpo2Latest — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest(new ApiItemResponse<Spo2ReadingDto> { ReturnCode = 400 });
        }

        Spo2ReadingDto? item;
        try
        {
            item = await _spo2Service.GetLatestAsync(deviceId, tz, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetSpo2Latest — DB read failed for device {DeviceId}", deviceId);
            return StatusCode(500, new ApiItemResponse<Spo2ReadingDto> { ReturnCode = 500 });
        }

        if (item is null)
        {
            _logger.LogInformation("GetSpo2Latest — no data for device {DeviceId}", deviceId);
            return NotFound(new ApiItemResponse<Spo2ReadingDto> { ReturnCode = 404 });
        }

        _logger.LogInformation(
            "GetSpo2Latest — exit, device: {DeviceId}, dataTime: {DataTime}",
            deviceId, item.DataTime);

        return Ok(new ApiItemResponse<Spo2ReadingDto>
        {
            ReturnCode = 0,
            Data = item
        });
    }
}
