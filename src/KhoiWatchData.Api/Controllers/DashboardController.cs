using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.Interfaces;

namespace KhoiWatchData.Api.Controllers;

[ApiController]
[Route("dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboard;

    public DashboardController(IDashboardService dashboard)
    {
        _dashboard = dashboard;
    }

    /// <summary>
    /// Returns high-level fleet summary counts for the admin dashboard header:
    /// total workers, active alerts (last 24h), SOS events (last 24h), and workers in distress.
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _dashboard.GetSummaryAsync();
        if (result.IsFailure)
            return StatusCode(500, new { message = result.Error });

        return Ok(result.Value);
    }
}
