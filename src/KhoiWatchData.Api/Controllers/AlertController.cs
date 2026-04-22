using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.Interfaces;

namespace KhoiWatchData.Api.Controllers;

[ApiController]
[Route("alerts")]
public sealed class AlertController : ControllerBase
{
    private readonly IAlertQueryService _alertService;

    public AlertController(IAlertQueryService alertService)
    {
        _alertService = alertService;
    }

    /// <summary>
    /// Returns recent device alarms enriched with the linked worker's name.
    /// Supports the Alerts / Safety Alerts page (Slide 6).
    /// </summary>
    /// <param name="hours">Look-back window in hours (default: 24, max: 720).</param>
    /// <param name="limit">Maximum records to return (default: 50, max: 500).</param>
    [HttpGet]
    public async Task<IActionResult> GetRecentAlarms(
        [FromQuery] int hours = 24,
        [FromQuery] int limit = 50)
    {
        var result = await _alertService.GetRecentAlarmsAsync(hours, limit);
        if (result.IsFailure)
            return StatusCode(500, new { message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Returns aggregate fleet status counts for the live monitoring header (Slide 8):
    /// total workers, active alerts (last 24h), and SOS events (last 24h).
    /// </summary>
    [HttpGet("fleet/status")]
    public async Task<IActionResult> GetFleetStatus()
    {
        var result = await _alertService.GetFleetStatusAsync();
        if (result.IsFailure)
            return StatusCode(500, new { message = result.Error });

        return Ok(result.Value);
    }
}
