using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.Interfaces;

namespace KhoiWatchData.Api.Controllers;

/// <summary>
/// Device command configuration queries — company-level and device-level.
/// </summary>
[ApiController]
[Route("")]
public class DeviceConfigController : ControllerBase
{
    private readonly IDeviceConfigQueryService _configService;

    public DeviceConfigController(IDeviceConfigQueryService configService)
    {
        _configService = configService;
    }

    // ── Company-level ─────────────────────────────────────────────────────────

    /// <summary>Command configurations for all active devices in a company, paginated.</summary>
    [HttpGet("companies/{companyId:int}/devices/config")]
    public async Task<IActionResult> GetByCompany(
        int companyId,
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _configService.GetByCompanyAsync(companyId, page, pageSize);
        if (result.IsFailure)
            return result.ErrorCode == 400 ? BadRequest(result.Error) : StatusCode(500, result.Error);

        return Ok(result.Value);
    }

    // ── Device-level ─────────────────────────────────────────────────────────

    /// <summary>Full command configuration for a single device.</summary>
    [HttpGet("devices/{deviceId}/config")]
    public async Task<IActionResult> GetByDevice(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest("Device ID is required.");

        var result = await _configService.GetByDeviceAsync(deviceId);
        if (result.IsFailure)
            return result.ErrorCode == 404 ? NotFound(result.Error) : StatusCode(500, result.Error);

        return Ok(result.Value);
    }
}
