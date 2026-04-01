using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Read-only device information endpoints consumed by mobile and web applications.
/// Routes:
///   GET /api/devices              — list all registered devices
///   GET /api/devices/{deviceId}   — single device detail
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("app-read")]
[ApiController]
[Route("api/v{version:apiVersion}/devices")]
public sealed class DeviceQueryController : ControllerBase
{
    private readonly IDeviceQueryService _deviceService;
    private readonly ILogger<DeviceQueryController> _logger;

    public DeviceQueryController(
        IDeviceQueryService deviceService,
        ILogger<DeviceQueryController> logger)
    {
        _deviceService = deviceService;
        _logger = logger;
    }

    /// <summary>Returns a summary list of all registered devices.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAllDevicesAsync([FromQuery] string? tz, CancellationToken ct)
    {
        _logger.LogInformation("GetAllDevices — entry");

        IReadOnlyList<DeviceSummaryDto> data;
        try
        {
            data = await _deviceService.GetAllDevicesAsync(tz, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllDevices — service call failed");
            return StatusCode(500, new ApiListResponse<DeviceSummaryDto> { ReturnCode = 500 });
        }

        _logger.LogInformation("GetAllDevices — exit, {Count} devices", data.Count);
        return Ok(new ApiListResponse<DeviceSummaryDto> { ReturnCode = 0, Count = data.Count, Data = data });
    }

    /// <summary>Returns full detail for a single device.</summary>
    [HttpGet("{deviceId}")]
    public async Task<IActionResult> GetDeviceAsync(
        string deviceId, [FromQuery] string? tz, CancellationToken ct)
    {
        _logger.LogInformation("GetDevice — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new ApiItemResponse<DeviceDetailDto> { ReturnCode = 400 });

        DeviceDetailDto? data;
        try
        {
            data = await _deviceService.GetDeviceAsync(deviceId, tz, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDevice — service call failed for device {DeviceId}", deviceId);
            return StatusCode(500, new ApiItemResponse<DeviceDetailDto> { ReturnCode = 500 });
        }

        if (data is null)
        {
            _logger.LogInformation("GetDevice — not found: {DeviceId}", deviceId);
            return NotFound(new ApiItemResponse<DeviceDetailDto> { ReturnCode = 404 });
        }

        _logger.LogInformation("GetDevice — exit, device: {DeviceId}", deviceId);
        return Ok(new ApiItemResponse<DeviceDetailDto> { ReturnCode = 0, Data = data });
    }
}
