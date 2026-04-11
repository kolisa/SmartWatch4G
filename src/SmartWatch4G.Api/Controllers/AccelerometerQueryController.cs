using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Read-only accelerometer endpoints consumed by mobile and web applications.
/// Route: GET /api/devices/{deviceId}/accelerometer?date=
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("dashboard-api")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/accelerometer")]
public sealed class AccelerometerQueryController : ControllerBase
{
    private readonly IAccelerometerQueryService _accService;
    private readonly ILogger<AccelerometerQueryController> _logger;
    private readonly IDateTimeService _dt;

    public AccelerometerQueryController(
        IAccelerometerQueryService accService,
        ILogger<AccelerometerQueryController> logger,
        IDateTimeService dt)
    {
        _accService = accService;
        _logger = logger;
        _dt = dt;
    }

    /// <summary>Returns accelerometer sample bursts for the given device and date.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAccelerometerAsync(
        string deviceId,
        [FromQuery] string date,
        [FromQuery] string? tz,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "GetAccelerometer — entry, device: {DeviceId}, date: {Date}", deviceId, date);

        if (string.IsNullOrWhiteSpace(deviceId) || !_dt.IsValidDate(date))
        {
            _logger.LogWarning(
                "GetAccelerometer — invalid parameters, device: {DeviceId}, date: {Date}",
                deviceId, date);
            return BadRequest(new ApiListResponse<AccelerometerReadingDto> { ReturnCode = 400 });
        }

        IReadOnlyList<AccelerometerReadingDto> data;
        try
        {
            data = await _accService.GetByDateAsync(deviceId, date, tz, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetAccelerometer — DB read failed for device {DeviceId}, date {Date}",
                deviceId, date);
            return StatusCode(500, new ApiListResponse<AccelerometerReadingDto> { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "GetAccelerometer — exit, device: {DeviceId}, date: {Date}, count: {Count}",
            deviceId, date, data.Count);

        return Ok(new ApiListResponse<AccelerometerReadingDto>
        {
            ReturnCode = 0,
            Count = data.Count,
            Data = data
        });
    }
}
