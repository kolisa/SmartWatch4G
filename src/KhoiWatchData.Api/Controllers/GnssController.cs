using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.Interfaces;

namespace KhoiWatchData.Api.Controllers;

[ApiController]
[Route("gnss")]
public sealed class GnssController : ControllerBase
{
    private readonly IGnssQueryService _gnssService;

    public GnssController(IGnssQueryService gnssService)
    {
        _gnssService = gnssService;
    }

    /// <summary>
    /// Returns the list of users whose devices are currently online,
    /// each paired with their most recent GNSS tracking record.
    /// Devices that are offline or have no tracking data are excluded.
    /// </summary>
    [HttpGet("online-users")]
    public async Task<IActionResult> GetOnlineDevices()
    {
        var result = await _gnssService.GetOnlineUsersWithTrackingAsync();
        if (result.IsFailure)
            return StatusCode(500, new { message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Returns the full GNSS coordinate history for a specific device,
    /// optionally filtered by date range. Works regardless of device online status.
    /// The current device status is included in the response.
    /// </summary>
    /// <param name="deviceId">The device identifier to query.</param>
    /// <param name="from">Optional start of date range (inclusive, UTC).</param>
    /// <param name="to">Optional end of date range (inclusive, UTC).</param>
    [HttpGet("{deviceId}/history")]
    public async Task<IActionResult> GetTrackHistory(
        string deviceId,
        [FromQuery] System.DateTime? from = null,
        [FromQuery] System.DateTime? to   = null)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new { message = "Device ID is required." });

        if (from.HasValue && to.HasValue && from.Value > to.Value)
            return BadRequest(new { message = "'from' must be earlier than or equal to 'to'." });

        var result = await _gnssService.GetTrackHistoryAsync(deviceId, from, to);
        if (result.IsFailure)
            return StatusCode(500, new { message = result.Error });

        return Ok(result.Value);
    }
}
