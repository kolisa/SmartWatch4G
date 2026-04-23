using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.Interfaces;

namespace KhoiWatchData.Api.Controllers;

/// <summary>
/// Generic device-centric API surface.
/// Route names are system-agnostic so this controller can be used in any integration.
/// </summary>
[ApiController]
[Route("devices")]
public sealed class DeviceController : ControllerBase
{
    private readonly IUserProfileQueryService _deviceService;

    public DeviceController(IUserProfileQueryService deviceService)
    {
        _deviceService = deviceService;
    }

    /// <summary>
    /// Returns a paginated list of registered devices with their latest health snapshot
    /// and GPS coordinates. Optionally filter by company.
    /// </summary>
    /// <param name="page">Page number, 1-based (default: 1).</param>
    /// <param name="pageSize">Items per page, max 100 (default: 10).</param>
    /// <param name="companyId">Optional company filter.</param>
    [HttpGet]
    public async Task<IActionResult> GetDevices(
        [FromQuery] int  page      = 1,
        [FromQuery] int  pageSize  = 10,
        [FromQuery] int? companyId = null)
    {
        var result = await _deviceService.GetPagedUserProfilesAsync(page, pageSize, companyId);
        if (result.IsFailure)
            return StatusCode(500, new { message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Returns the full profile and latest sensor data for a single device.
    /// </summary>
    /// <param name="deviceId">Unique device identifier.</param>
    [HttpGet("{deviceId}")]
    public async Task<IActionResult> GetDevice(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new { message = "Device ID is required." });

        var result = await _deviceService.GetUserProfileDetailAsync(deviceId);
        if (result.IsFailure)
            return result.ErrorCode switch
            {
                404 => NotFound(new { message = result.Error }),
                _   => StatusCode(500, new { message = result.Error })
            };

        return Ok(result.Value);
    }

    /// <summary>
    /// Returns the latest operational telemetry for all registered devices in a single call.
    /// Each entry includes device status, battery, vitals, and GPS coordinates.
    /// Useful for populating live-monitoring dashboards with large device fleets.
    /// Optionally filter by company.
    /// </summary>
    /// <param name="companyId">Optional company filter.</param>
    [HttpGet("telemetry")]
    public async Task<IActionResult> GetTelemetry([FromQuery] int? companyId = null)
    {
        var result = await _deviceService.GetAllDeviceTelemetryAsync(companyId);
        if (result.IsFailure)
            return StatusCode(500, new { message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Returns the latest operational telemetry for a single device:
    /// online status, battery, heart rate, SpO2, blood pressure, fatigue, steps, and GPS.
    /// Intended for high-frequency polling by integrations that only need the live data feed.
    /// </summary>
    /// <param name="deviceId">Unique device identifier.</param>
    [HttpGet("{deviceId}/telemetry")]
    public async Task<IActionResult> GetDeviceTelemetry(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new { message = "Device ID is required." });

        var result = await _deviceService.GetDeviceTelemetryAsync(deviceId);
        if (result.IsFailure)
            return result.ErrorCode switch
            {
                404 => NotFound(new { message = result.Error }),
                _   => StatusCode(500, new { message = result.Error })
            };

        return Ok(result.Value);
    }
}
