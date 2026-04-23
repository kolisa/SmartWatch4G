using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;

namespace KhoiWatchData.Api.Controllers;

/// <summary>
/// GPS track queries — company-level and device-level with pagination,
/// date filtering, and online/offline separation.
/// </summary>
[ApiController]
[Route("")]
public class GpsController : ControllerBase
{
    private readonly IGpsQueryService _gpsService;

    public GpsController(IGpsQueryService gpsService)
    {
        _gpsService = gpsService;
    }

    // ── Company-level ─────────────────────────────────────────────────────────

    /// <summary>All GPS tracks for a company, paginated. Includes online/offline device counts.</summary>
    [HttpGet("companies/{companyId:int}/gps")]
    public async Task<IActionResult> GetByCompany(int companyId, [FromQuery] GpsQueryParams q)
    {
        var result = await _gpsService.GetByCompanyAsync(companyId, q);
        if (result.IsFailure)
            return result.ErrorCode == 400 ? BadRequest(result.Error) : StatusCode(500, result.Error);

        return Ok(result.Value);
    }

    /// <summary>GPS tracks for online devices in a company only.</summary>
    [HttpGet("companies/{companyId:int}/gps/online")]
    public async Task<IActionResult> GetOnlineByCompany(int companyId, [FromQuery] GpsQueryParams q)
    {
        var result = await _gpsService.GetOnlineByCompanyAsync(companyId, q);
        if (result.IsFailure)
            return result.ErrorCode == 400 ? BadRequest(result.Error) : StatusCode(500, result.Error);

        return Ok(result.Value);
    }

    /// <summary>GPS tracks for offline devices in a company only.</summary>
    [HttpGet("companies/{companyId:int}/gps/offline")]
    public async Task<IActionResult> GetOfflineByCompany(int companyId, [FromQuery] GpsQueryParams q)
    {
        var result = await _gpsService.GetOfflineByCompanyAsync(companyId, q);
        if (result.IsFailure)
            return result.ErrorCode == 400 ? BadRequest(result.Error) : StatusCode(500, result.Error);

        return Ok(result.Value);
    }

    // ── Device-level ─────────────────────────────────────────────────────────

    /// <summary>GPS track history for a single device.</summary>
    [HttpGet("devices/{deviceId}/gps")]
    public async Task<IActionResult> GetByDevice(string deviceId, [FromQuery] GpsQueryParams q)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest("Device ID is required.");

        var result = await _gpsService.GetByDeviceAsync(deviceId, q);
        if (result.IsFailure)
        {
            if (result.ErrorCode == 404) return NotFound(result.Error);
            if (result.ErrorCode == 400) return BadRequest(result.Error);
            return StatusCode(500, result.Error);
        }

        return Ok(result.Value);
    }
}
