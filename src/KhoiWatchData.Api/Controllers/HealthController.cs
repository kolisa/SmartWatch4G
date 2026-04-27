using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;

namespace KhoiWatchData.Api.Controllers;

/// <summary>
/// Health data queries — company-level (paged + summary) and device-level with pagination and date filtering.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
public class HealthController : ControllerBase
{
    private readonly IHealthQueryService _healthService;

    public HealthController(IHealthQueryService healthService)
    {
        _healthService = healthService;
    }

    // ── Company-level ─────────────────────────────────────────────────────────

    /// <summary>Paged health records for all devices in a company with optional date filters.</summary>
    [HttpGet("companies/{companyId:int}/health")]
    public async Task<IActionResult> GetByCompany(int companyId, [FromQuery] HealthQueryParams q)
    {
        var result = await _healthService.GetByCompanyAsync(companyId, q);
        if (result.IsFailure)
            return result.ErrorCode == 400 ? BadRequest(result.Error) : StatusCode(500, result.Error);

        return Ok(result.Value);
    }

    /// <summary>Per-device health aggregates (averages, totals) for a company.</summary>
    [HttpGet("companies/{companyId:int}/health/summary")]
    public async Task<IActionResult> GetSummaryByCompany(
        int companyId,
        [FromQuery] System.DateTime? from,
        [FromQuery] System.DateTime? to)
    {
        var result = await _healthService.GetSummaryByCompanyAsync(companyId, from, to);
        if (result.IsFailure)
            return result.ErrorCode == 400 ? BadRequest(result.Error) : StatusCode(500, result.Error);

        return Ok(result.Value);
    }

    // ── Device-level ─────────────────────────────────────────────────────────

    /// <summary>Paged health records for a single device with optional date filters.</summary>
    [HttpGet("devices/{deviceId}/health")]
    public async Task<IActionResult> GetByDevice(string deviceId, [FromQuery] HealthQueryParams q)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest("Device ID is required.");

        var result = await _healthService.GetByDeviceAsync(deviceId, q);
        if (result.IsFailure)
        {
            if (result.ErrorCode == 404) return NotFound(result.Error);
            if (result.ErrorCode == 400) return BadRequest(result.Error);
            return StatusCode(500, result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>Latest single health snapshot for a device.</summary>
    [HttpGet("devices/{deviceId}/health/latest")]
    public async Task<IActionResult> GetLatestByDevice(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest("Device ID is required.");

        var result = await _healthService.GetLatestByDeviceAsync(deviceId);
        if (result.IsFailure)
            return result.ErrorCode == 404 ? NotFound(result.Error) : StatusCode(500, result.Error);

        return Ok(result.Value);
    }
}
