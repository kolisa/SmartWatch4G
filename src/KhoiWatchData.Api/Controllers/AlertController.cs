using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.Interfaces;

namespace KhoiWatchData.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/alerts")]
public sealed class AlertController : ControllerBase
{
    private readonly IAlertQueryService _alertService;

    public AlertController(IAlertQueryService alertService)
    {
        _alertService = alertService;
    }

    /// <summary>
    /// Returns recent device alerts enriched with the linked user's name.
    /// </summary>
    /// <param name="withinHours">Look-back window in hours (default: 24, max: 720).</param>
    /// <param name="limit">Maximum records to return (default: 50, max: 500).</param>
    [HttpGet]
    public async Task<IActionResult> GetRecentAlerts(
        [FromQuery] int withinHours = 24,
        [FromQuery] int limit = 50)
    {
        var result = await _alertService.GetRecentAlertsAsync(withinHours, limit);
        if (result.IsFailure)
            return StatusCode(500, new { message = result.Error });

        return Ok(result.Value);
    }
}
