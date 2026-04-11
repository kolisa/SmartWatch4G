using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Read-only RRI (R-to-R interval / HRV) endpoints consumed by mobile and web applications.
/// Route: GET /api/devices/{deviceId}/rri?date=
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("dashboard-api")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/rri")]
public sealed class RriQueryController : ControllerBase
{
    private readonly IRriQueryService _rriService;
    private readonly ILogger<RriQueryController> _logger;
    private readonly IDateTimeService _dt;

    public RriQueryController(
        IRriQueryService rriService,
        ILogger<RriQueryController> logger,
        IDateTimeService dt)
    {
        _rriService = rriService;
        _logger = logger;
        _dt = dt;
    }

    /// <summary>Returns RRI readings for the given device and date.</summary>
    [HttpGet]
    public async Task<IActionResult> GetRriAsync(
        string deviceId,
        [FromQuery] string date,
        [FromQuery] string? tz,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "GetRri — entry, device: {DeviceId}, date: {Date}", deviceId, date);

        if (string.IsNullOrWhiteSpace(deviceId) || !_dt.IsValidDate(date))
        {
            _logger.LogWarning(
                "GetRri — invalid parameters, device: {DeviceId}, date: {Date}", deviceId, date);
            return BadRequest(new ApiListResponse<RriReadingDto> { ReturnCode = 400 });
        }

        IReadOnlyList<RriReadingDto> data;
        try
        {
            data = await _rriService.GetByDateAsync(deviceId, date, tz, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetRri — DB read failed for device {DeviceId}, date {Date}", deviceId, date);
            return StatusCode(500, new ApiListResponse<RriReadingDto> { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "GetRri — exit, device: {DeviceId}, date: {Date}, count: {Count}",
            deviceId, date, data.Count);

        return Ok(new ApiListResponse<RriReadingDto>
        {
            ReturnCode = 0,
            Count = data.Count,
            Data = data
        });
    }
}
