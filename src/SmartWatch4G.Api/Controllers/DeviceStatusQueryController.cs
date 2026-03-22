using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Read-only device-status endpoints consumed by mobile and web applications.
/// Routes:
///   GET /api/devices/{deviceId}/status          — status events for a date
///   GET /api/devices/{deviceId}/status/latest   — most recent status event
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("app-read")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/status")]
public sealed class DeviceStatusQueryController : ControllerBase
{
    private readonly IDeviceStatusRepository _statusRepo;
    private readonly ILogger<DeviceStatusQueryController> _logger;

    public DeviceStatusQueryController(
        IDeviceStatusRepository statusRepo,
        ILogger<DeviceStatusQueryController> logger)
    {
        _statusRepo = statusRepo;
        _logger = logger;
    }

    /// <summary>Returns device status events received on the given date (yyyy-MM-dd).</summary>
    [HttpGet]
    public async Task<IActionResult> GetDeviceStatusAsync(
        string deviceId,
        [FromQuery] string date,
        [FromQuery] string? tz,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "GetDeviceStatus — entry, device: {DeviceId}, date: {Date}", deviceId, date);
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        if (string.IsNullOrWhiteSpace(deviceId) || !DateTimeUtilities.IsValidDate(date))
        {
            _logger.LogWarning(
                "GetDeviceStatus — invalid parameters, device: {DeviceId}, date: {Date}",
                deviceId, date);
            return BadRequest(new ApiListResponse<DeviceStatusItemDto> { ReturnCode = 400 });
        }

        IReadOnlyList<DeviceStatusRecord> records;
        try
        {
            records = await _statusRepo.GetByDeviceAndDateAsync(deviceId, date, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetDeviceStatus — DB read failed for device {DeviceId}, date {Date}",
                deviceId, date);
            return StatusCode(500, new ApiListResponse<DeviceStatusItemDto> { ReturnCode = 500 });
        }

        var data = records.Select(r => new DeviceStatusItemDto
        {
            DeviceId  = r.DeviceId,
            EventTime = DateTimeUtilities.LocalizeTimestamp(r.EventTime, tzInfo),
            Status    = r.Status,
            ReceivedAt = DateTimeUtilities.LocalizeDateTime(r.ReceivedAt, tzInfo)
        }).ToList();

        _logger.LogInformation(
            "GetDeviceStatus — exit, device: {DeviceId}, date: {Date}, count: {Count}",
            deviceId, date, data.Count);

        return Ok(new ApiListResponse<DeviceStatusItemDto>
        {
            ReturnCode = 0,
            Count = data.Count,
            Data = data
        });
    }

    /// <summary>Returns the single most recent status event for a device.</summary>
    [HttpGet("latest")]
    public async Task<IActionResult> GetDeviceStatusLatestAsync(string deviceId, [FromQuery] string? tz, CancellationToken ct)
    {
        _logger.LogInformation("GetDeviceStatusLatest — entry, device: {DeviceId}", deviceId);
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest(new ApiItemResponse<DeviceStatusItemDto> { ReturnCode = 400 });
        }

        DeviceStatusRecord? record;
        try
        {
            record = await _statusRepo.GetLatestByDeviceAsync(deviceId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetDeviceStatusLatest — DB read failed for device {DeviceId}", deviceId);
            return StatusCode(500, new ApiItemResponse<DeviceStatusItemDto> { ReturnCode = 500 });
        }

        if (record is null)
        {
            _logger.LogInformation("GetDeviceStatusLatest — no data for device {DeviceId}", deviceId);
            return NotFound(new ApiItemResponse<DeviceStatusItemDto> { ReturnCode = 404 });
        }

        _logger.LogInformation(
            "GetDeviceStatusLatest — exit, device: {DeviceId}, eventTime: {EventTime}",
            deviceId, record.EventTime);

        return Ok(new ApiItemResponse<DeviceStatusItemDto>
        {
            ReturnCode = 0,
            Data = new DeviceStatusItemDto
            {
                DeviceId  = record.DeviceId,
                EventTime = DateTimeUtilities.LocalizeTimestamp(record.EventTime, tzInfo),
                Status    = record.Status,
                ReceivedAt = DateTimeUtilities.LocalizeDateTime(record.ReceivedAt, tzInfo)
            }
        });
    }
}
