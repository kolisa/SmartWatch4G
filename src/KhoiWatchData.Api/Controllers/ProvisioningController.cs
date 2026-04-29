using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.Interfaces;

namespace KhoiWatchData.Api.Controllers;

/// <summary>
/// Pushes the five core Iwown command settings to one or all devices.
/// Commands: cmd/datafreq, cmd/gps/locate, cmd/datasync,
///           cmd/realtime/location, device/status.
/// Each command is retried up to 3 times (60-second delay) until the API
/// returns ReturnCode 0. Settings are saved locally only on success.
/// Runs automatically 3× daily (06:00, 12:00, 18:00 UTC) via DeviceProvisioningJob.
/// </summary>
[ApiVersionNeutral]
[ApiController]
[Route("provisioning")]
public sealed class ProvisioningController : ControllerBase
{
    private readonly IDeviceProvisioningService _provisioning;

    public ProvisioningController(IDeviceProvisioningService provisioning)
    {
        _provisioning = provisioning;
    }

    /// <summary>
    /// Provisions ALL active devices — optionally filtered by company.
    /// Skips cmd/datafreq and cmd/gps/locate for devices that already have
    /// confirmed settings in the database.
    /// </summary>
    /// <param name="companyId">Optional. Only provision devices linked to this company.</param>
    [HttpPost("devices")]
    public async Task<IActionResult> ProvisionAll([FromQuery] int? companyId = null)
    {
        var report = await _provisioning.ProvisionAllAsync(companyId);
        return Ok(report);
    }

    /// <summary>
    /// Provisions a single device by IMEI.
    /// Returns 404 if the device is not registered.
    /// </summary>
    /// <param name="deviceId">Device IMEI.</param>
    [HttpPost("devices/{deviceId}")]
    public async Task<IActionResult> ProvisionDevice(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new { message = "Device ID is required." });

        var result = await _provisioning.ProvisionDeviceAsync(deviceId);
        return Ok(result);
    }
}
