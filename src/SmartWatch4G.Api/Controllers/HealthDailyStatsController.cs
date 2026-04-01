using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Application.Utilities;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Returns aggregated daily health statistics computed from 1-minute health snapshots.
/// Route: GET /api/devices/{deviceId}/health/daily-stats?date=
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("app-read")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/health/daily-stats")]
public sealed class HealthDailyStatsController : ControllerBase
{
    private readonly IHealthQueryService _healthService;
    private readonly ILogger<HealthDailyStatsController> _logger;

    public HealthDailyStatsController(
        IHealthQueryService healthService,
        ILogger<HealthDailyStatsController> logger)
    {
        _healthService = healthService;
        _logger = logger;
    }

    /// <summary>
    /// Returns min/max/avg heart rate, avg SpO2, total steps, calories, distance,
    /// avg temperature, avg BP, avg HRV, and avg fatigue for the given date.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDailyStatsAsync(
        string deviceId,
        [FromQuery] string date,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "GetDailyStats — entry, device: {DeviceId}, date: {Date}", deviceId, date);

        if (string.IsNullOrWhiteSpace(deviceId) || !DateTimeUtilities.IsValidDate(date))
        {
            _logger.LogWarning(
                "GetDailyStats — invalid parameters, device: {DeviceId}, date: {Date}",
                deviceId, date);
            return BadRequest(new ApiItemResponse<HealthDailyStatsDto> { ReturnCode = 400 });
        }

        HealthDailyStatsDto? stats;
        try
        {
            stats = await _healthService.GetDailyStatsAsync(deviceId, date, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetDailyStats — DB read failed for device {DeviceId}, date {Date}",
                deviceId, date);
            return StatusCode(500, new ApiItemResponse<HealthDailyStatsDto> { ReturnCode = 500 });
        }

        if (stats is null)
        {
            _logger.LogInformation(
                "GetDailyStats — no data for device {DeviceId}, date {Date}", deviceId, date);
            return NotFound(new ApiItemResponse<HealthDailyStatsDto> { ReturnCode = 404 });
        }

        _logger.LogInformation(
            "GetDailyStats — exit, device: {DeviceId}, date: {Date}",
            deviceId, date);

        return Ok(new ApiItemResponse<HealthDailyStatsDto>
        {
            ReturnCode = 0,
            Data = stats
        });
    }
}
