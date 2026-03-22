using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Read-only SpO2 (blood-oxygen) endpoints consumed by mobile and web applications.
/// Routes:
///   GET /api/devices/{deviceId}/spo2          — history (?date= or ?from=&amp;to=)
///   GET /api/devices/{deviceId}/spo2/latest   — most recent reading
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("app-read")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/spo2")]
public sealed class Spo2QueryController : ControllerBase
{
    private readonly ISpo2DataRepository _spo2Repo;
    private readonly ILogger<Spo2QueryController> _logger;

    public Spo2QueryController(
        ISpo2DataRepository spo2Repo,
        ILogger<Spo2QueryController> logger)
    {
        _spo2Repo = spo2Repo;
        _logger = logger;
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
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest(new ApiListResponse<Spo2ReadingDto> { ReturnCode = 400 });
        }

        IReadOnlyList<Spo2DataRecord> records;
        string filterDesc;

        try
        {
            if (!string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(to))
            {
                if (!DateTimeUtilities.IsValidDateTime(from) || !DateTimeUtilities.IsValidDateTime(to))
                {
                    _logger.LogWarning(
                        "GetSpo2 — invalid datetime range, from: {From}, to: {To}", from, to);
                    return BadRequest(new ApiListResponse<Spo2ReadingDto> { ReturnCode = 400 });
                }

                filterDesc = $"{from} → {to}";
                records = await _spo2Repo.GetByDeviceAndDateRangeAsync(deviceId, from, to, ct)
                    .ConfigureAwait(false);
            }
            else if (DateTimeUtilities.IsValidDate(date))
            {
                (string dayFrom, string dayTo) = DateTimeUtilities.ToDayRange(date);
                filterDesc = $"date {date}";
                records = await _spo2Repo.GetByDeviceAndDateRangeAsync(deviceId, dayFrom, dayTo, ct)
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

        var data = records.Select(r => new Spo2ReadingDto
        {
            DeviceId = r.DeviceId ?? string.Empty,
            DataTime = DateTimeUtilities.LocalizeTimestamp(r.DataTime, tzInfo),
            Spo2 = r.Spo2,
            HeartRate = r.HeartRate,
            Perfusion = r.Perfusion,
            Touch = r.Touch
        }).ToList();

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
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest(new ApiItemResponse<Spo2ReadingDto> { ReturnCode = 400 });
        }

        Spo2DataRecord? record;
        try
        {
            record = await _spo2Repo.GetLatestByDeviceAsync(deviceId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetSpo2Latest — DB read failed for device {DeviceId}", deviceId);
            return StatusCode(500, new ApiItemResponse<Spo2ReadingDto> { ReturnCode = 500 });
        }

        if (record is null)
        {
            _logger.LogInformation("GetSpo2Latest — no data for device {DeviceId}", deviceId);
            return NotFound(new ApiItemResponse<Spo2ReadingDto> { ReturnCode = 404 });
        }

        _logger.LogInformation(
            "GetSpo2Latest — exit, device: {DeviceId}, dataTime: {DataTime}",
            deviceId, record.DataTime);

        return Ok(new ApiItemResponse<Spo2ReadingDto>
        {
            ReturnCode = 0,
            Data = new Spo2ReadingDto
            {
                DeviceId = record.DeviceId ?? string.Empty,
                DataTime = DateTimeUtilities.LocalizeTimestamp(record.DataTime, tzInfo),
                Spo2 = record.Spo2,
                HeartRate = record.HeartRate,
                Perfusion = record.Perfusion,
                Touch = record.Touch
            }
        });
    }
}
