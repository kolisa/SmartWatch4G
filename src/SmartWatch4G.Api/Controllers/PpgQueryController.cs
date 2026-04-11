using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Read-only PPG (photoplethysmography) data query endpoint.
/// Route: GET /api/v1/devices/{deviceId}/ppg?date=
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("dashboard-api")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/ppg")]
public sealed class PpgQueryController : ControllerBase
{
    private readonly IPpgQueryService _ppgService;
    private readonly ILogger<PpgQueryController> _logger;
    private readonly IDateTimeService _dt;

    public PpgQueryController(IPpgQueryService ppgService, ILogger<PpgQueryController> logger, IDateTimeService dt)
    {
        _ppgService = ppgService;
        _logger = logger;
        _dt = dt;
    }

    /// <summary>Returns PPG records for the given device and date.</summary>
    [HttpGet]
    public async Task<IActionResult> GetPpgAsync(
        string deviceId,
        [FromQuery] string date,
        [FromQuery] string? tz,
        CancellationToken ct)
    {
        _logger.LogInformation("GetPpg — entry, device: {DeviceId}, date: {Date}", deviceId, date);

        if (string.IsNullOrWhiteSpace(deviceId) || !_dt.IsValidDate(date))
            return BadRequest(new ApiListResponse<PpgReadingDto> { ReturnCode = 400 });

        IReadOnlyList<PpgReadingDto> data;
        try
        {
            data = await _ppgService.GetByDateAsync(deviceId, date, tz, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPpg — DB read failed for device {DeviceId}, date {Date}", deviceId, date);
            return StatusCode(500, new ApiListResponse<PpgReadingDto> { ReturnCode = 500 });
        }

        _logger.LogInformation("GetPpg — exit, device: {DeviceId}, date: {Date}, count: {Count}", deviceId, date, data.Count);
        return Ok(new ApiListResponse<PpgReadingDto> { ReturnCode = 0, Count = data.Count, Data = data });
    }
}
