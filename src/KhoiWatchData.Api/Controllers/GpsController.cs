using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;

namespace KhoiWatchData.Api.Controllers;

/// <summary>
/// GPS track queries — company-level and device-level with pagination,
/// date filtering, and online/offline separation.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
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

    // ── Map view ─────────────────────────────────────────────────────────────

    /// <summary>
    /// All GPS tracks and latest health snapshot for every device in a company on a given date.
    /// Defaults to today when no date is supplied.
    /// Tracks are ordered newest-first so index 0 is the current position marker.
    /// </summary>
    /// <param name="companyId">Company to query.</param>
    /// <param name="date">Date to filter (UTC date portion only). Defaults to today.</param>
    [HttpGet("companies/{companyId:int}/map")]
    public async Task<IActionResult> GetMapData(int companyId, [FromQuery] System.DateTime? date = null)
    {
        var result = await _gpsService.GetMapDataAsync(companyId, date);
        if (result.IsFailure)
            return StatusCode(500, new { message = result.Error });

        return Ok(result.Value);
    }

    // ── Device-level ─────────────────────────────────────────────────────────

    /// <summary>
    /// All GPS tracks and latest health snapshot for a single device on a given date.
    /// Defaults to today when no date is supplied.
    /// Tracks are ordered newest-first so index 0 is the current position marker.
    /// </summary>
    [HttpGet("devices/{deviceId}/map")]
    public async Task<IActionResult> GetDeviceMapData(string deviceId, [FromQuery] System.DateTime? date = null)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new { message = "Device ID is required." });

        var result = await _gpsService.GetDeviceMapDataAsync(deviceId, date);
        if (result.IsFailure)
        {
            if (result.ErrorCode == 404) return NotFound(new { message = result.Error });
            return StatusCode(500, new { message = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Online/offline status and latest GPS position for a single device.
    /// StatusCode 1 = online, 0 = offline.
    /// </summary>
    [HttpGet("devices/{deviceId}/gps/online")]
    public async Task<IActionResult> GetDeviceGpsStatus(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest("Device ID is required.");

        var result = await _gpsService.GetDeviceGpsStatusAsync(deviceId);
        if (result.IsFailure)
            return StatusCode(500, result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// GPS track history for a single device, paginated with optional date filter.
    /// Defaults to the current day when no date range is supplied.
    /// </summary>
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
