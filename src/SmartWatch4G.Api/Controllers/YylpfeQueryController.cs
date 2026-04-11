using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Read-only YYLPFE physiological feature data query endpoint.
/// Route: GET /api/v1/devices/{deviceId}/yylpfe?date=
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("dashboard-api")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/yylpfe")]
public sealed class YylpfeQueryController : ControllerBase
{
    private readonly IYylpfeQueryService _service;
    private readonly ILogger<YylpfeQueryController> _logger;
    private readonly IDateTimeService _dt;

    public YylpfeQueryController(
        IYylpfeQueryService service,
        ILogger<YylpfeQueryController> logger,
        IDateTimeService dt)
    {
        _service = service;
        _logger = logger;
        _dt = dt;
    }

    /// <summary>Returns YYLPFE records for the given device and date.</summary>
    [HttpGet]
    public async Task<IActionResult> GetYylpfeAsync(
        string deviceId,
        [FromQuery] string date,
        [FromQuery] string? tz,
        CancellationToken ct)
    {
        _logger.LogInformation("GetYylpfe — entry, device: {DeviceId}, date: {Date}", deviceId, date);

        if (string.IsNullOrWhiteSpace(deviceId) || !_dt.IsValidDate(date))
            return BadRequest(new ApiListResponse<YylpfeReadingDto> { ReturnCode = 400 });

        IReadOnlyList<YylpfeReadingDto> data;
        try
        {
            data = await _service.GetByDateAsync(deviceId, date, tz, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetYylpfe — DB read failed for device {DeviceId}, date {Date}", deviceId, date);
            return StatusCode(500, new ApiListResponse<YylpfeReadingDto> { ReturnCode = 500 });
        }

        _logger.LogInformation("GetYylpfe — exit, device: {DeviceId}, date: {Date}, count: {Count}", deviceId, date, data.Count);
        return Ok(new ApiListResponse<YylpfeReadingDto> { ReturnCode = 0, Count = data.Count, Data = data });
    }
}
