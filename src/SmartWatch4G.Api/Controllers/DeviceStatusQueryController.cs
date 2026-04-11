using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Read-only device-status endpoints consumed by mobile and web applications.
/// Routes:
///   GET /api/devices/{deviceId}/status          — status events for a date
///   GET /api/devices/{deviceId}/status/latest   — most recent status event
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("dashboard-api")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/status")]
public sealed class DeviceStatusQueryController : ControllerBase
{
    private readonly IDeviceQueryService _deviceService;
    private readonly ILogger<DeviceStatusQueryController> _logger;
    private readonly IDateTimeService _dt;

    public DeviceStatusQueryController(
        IDeviceQueryService deviceService,
        ILogger<DeviceStatusQueryController> logger,
        IDateTimeService dt)
    {
        _deviceService = deviceService;
        _logger = logger;
        _dt = dt;
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

        if (string.IsNullOrWhiteSpace(deviceId) || !_dt.IsValidDate(date))
        {
            _logger.LogWarning(
                "GetDeviceStatus — invalid parameters, device: {DeviceId}, date: {Date}",
                deviceId, date);
            return BadRequest(new ApiListResponse<DeviceStatusItemDto> { ReturnCode = 400 });
        }

        IReadOnlyList<DeviceStatusItemDto> data;
        try
        {
            data = await _deviceService.GetStatusByDateAsync(deviceId, date, tz, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetDeviceStatus — DB read failed for device {DeviceId}, date {Date}",
                deviceId, date);
            return StatusCode(500, new ApiListResponse<DeviceStatusItemDto> { ReturnCode = 500 });
        }

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

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest(new ApiItemResponse<DeviceStatusItemDto> { ReturnCode = 400 });
        }

        DeviceStatusItemDto? item;
        try
        {
            item = await _deviceService.GetLatestStatusAsync(deviceId, tz, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetDeviceStatusLatest — DB read failed for device {DeviceId}", deviceId);
            return StatusCode(500, new ApiItemResponse<DeviceStatusItemDto> { ReturnCode = 500 });
        }

        if (item is null)
        {
            _logger.LogInformation("GetDeviceStatusLatest — no data for device {DeviceId}", deviceId);
            return NotFound(new ApiItemResponse<DeviceStatusItemDto> { ReturnCode = 404 });
        }

        _logger.LogInformation(
            "GetDeviceStatusLatest — exit, device: {DeviceId}, eventTime: {EventTime}",
            deviceId, item.EventTime);

        return Ok(new ApiItemResponse<DeviceStatusItemDto>
        {
            ReturnCode = 0,
            Data = item
        });
    }
}
