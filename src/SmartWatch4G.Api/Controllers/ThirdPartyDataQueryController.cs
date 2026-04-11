using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Read-only third-party paired device data query endpoint.
/// Route: GET /api/v1/devices/{deviceId}/third-party?date=
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("dashboard-api")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/third-party")]
public sealed class ThirdPartyDataQueryController : ControllerBase
{
    private readonly IThirdPartyDataQueryService _service;
    private readonly ILogger<ThirdPartyDataQueryController> _logger;
    private readonly IDateTimeService _dt;

    public ThirdPartyDataQueryController(
        IThirdPartyDataQueryService service,
        ILogger<ThirdPartyDataQueryController> logger,
        IDateTimeService dt)
    {
        _service = service;
        _logger = logger;
        _dt = dt;
    }

    /// <summary>Returns third-party device data for the given device and date.</summary>
    [HttpGet]
    public async Task<IActionResult> GetThirdPartyDataAsync(
        string deviceId,
        [FromQuery] string date,
        [FromQuery] string? tz,
        CancellationToken ct)
    {
        _logger.LogInformation("GetThirdPartyData — entry, device: {DeviceId}, date: {Date}", deviceId, date);

        if (string.IsNullOrWhiteSpace(deviceId) || !_dt.IsValidDate(date))
            return BadRequest(new ApiListResponse<ThirdPartyDataDto> { ReturnCode = 400 });

        IReadOnlyList<ThirdPartyDataDto> data;
        try
        {
            data = await _service.GetByDateAsync(deviceId, date, tz, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetThirdPartyData — DB read failed for device {DeviceId}, date {Date}", deviceId, date);
            return StatusCode(500, new ApiListResponse<ThirdPartyDataDto> { ReturnCode = 500 });
        }

        _logger.LogInformation("GetThirdPartyData — exit, device: {DeviceId}, date: {Date}, count: {Count}", deviceId, date, data.Count);
        return Ok(new ApiListResponse<ThirdPartyDataDto> { ReturnCode = 0, Count = data.Count, Data = data });
    }
}
