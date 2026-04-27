using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.Interfaces;

namespace KhoiWatchData.Api.Controllers;

/// <summary>
/// Fleet-level summary API.
/// Consolidates the dashboard summary and alert fleet-status into a single,
/// system-agnostic endpoint usable by any integration.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/fleet")]
public sealed class FleetController : ControllerBase
{
    private readonly IDashboardService _dashboard;

    public FleetController(IDashboardService dashboard)
    {
        _dashboard = dashboard;
    }

    /// <summary>
    /// Returns aggregate fleet counts over the last 24 hours:
    /// total registered devices, active alerts, SOS events, and devices in distress.
    /// Optionally filter by company.
    /// </summary>
    /// <param name="companyId">Optional company filter.</param>
    [HttpGet("summary")]
    public async Task<IActionResult> GetFleetSummary([FromQuery] int? companyId = null)
    {
        var result = await _dashboard.GetSummaryAsync(companyId);
        if (result.IsFailure)
            return StatusCode(500, new { message = result.Error });

        return Ok(result.Value);
    }
}
