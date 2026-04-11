using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Read-only Multi-Leads ECG data query endpoint.
/// Route: GET /api/v1/devices/{deviceId}/ecg/multileads?date=
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("dashboard-api")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/ecg/multileads")]
public sealed class MultiLeadsEcgQueryController : ControllerBase
{
    private readonly IMultiLeadsEcgQueryService _service;
    private readonly ILogger<MultiLeadsEcgQueryController> _logger;
    private readonly IDateTimeService _dt;

    public MultiLeadsEcgQueryController(
        IMultiLeadsEcgQueryService service,
        ILogger<MultiLeadsEcgQueryController> logger,
        IDateTimeService dt)
    {
        _service = service;
        _logger = logger;
        _dt = dt;
    }

    /// <summary>Returns Multi-Leads ECG records for the given device and date.</summary>
    [HttpGet]
    public async Task<IActionResult> GetMultiLeadsEcgAsync(
        string deviceId,
        [FromQuery] string date,
        [FromQuery] string? tz,
        CancellationToken ct)
    {
        _logger.LogInformation("GetMultiLeadsEcg — entry, device: {DeviceId}, date: {Date}", deviceId, date);

        if (string.IsNullOrWhiteSpace(deviceId) || !_dt.IsValidDate(date))
            return BadRequest(new ApiListResponse<MultiLeadsEcgDto> { ReturnCode = 400 });

        IReadOnlyList<MultiLeadsEcgDto> data;
        try
        {
            data = await _service.GetByDateAsync(deviceId, date, tz, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetMultiLeadsEcg — DB read failed for device {DeviceId}, date {Date}", deviceId, date);
            return StatusCode(500, new ApiListResponse<MultiLeadsEcgDto> { ReturnCode = 500 });
        }

        _logger.LogInformation("GetMultiLeadsEcg — exit, device: {DeviceId}, date: {Date}, count: {Count}", deviceId, date, data.Count);
        return Ok(new ApiListResponse<MultiLeadsEcgDto> { ReturnCode = 0, Count = data.Count, Data = data });
    }
}
