using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Read-only accelerometer endpoints consumed by mobile and web applications.
/// Route: GET /api/devices/{deviceId}/accelerometer?date=
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("app-read")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/accelerometer")]
public sealed class AccelerometerQueryController : ControllerBase
{
    private readonly IAccDataRepository _accRepo;
    private readonly ILogger<AccelerometerQueryController> _logger;

    public AccelerometerQueryController(
        IAccDataRepository accRepo,
        ILogger<AccelerometerQueryController> logger)
    {
        _accRepo = accRepo;
        _logger = logger;
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
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        if (string.IsNullOrWhiteSpace(deviceId) || !DateTimeUtilities.IsValidDate(date))
        {
            _logger.LogWarning(
                "GetAccelerometer — invalid parameters, device: {DeviceId}, date: {Date}",
                deviceId, date);
            return BadRequest(new ApiListResponse<AccelerometerReadingDto> { ReturnCode = 400 });
        }

        (string from, string to) = DateTimeUtilities.ToDayRange(date);

        IReadOnlyList<AccDataRecord> records;
        try
        {
            records = await _accRepo.GetByDeviceAndDateRangeAsync(deviceId, from, to, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetAccelerometer — DB read failed for device {DeviceId}, date {Date}",
                deviceId, date);
            return StatusCode(500, new ApiListResponse<AccelerometerReadingDto> { ReturnCode = 500 });
        }

        var data = records.Select(r => new AccelerometerReadingDto
        {
            DeviceId = r.DeviceId ?? string.Empty,
            DataTime = DateTimeUtilities.LocalizeTimestamp(r.DataTime, tzInfo),
            SampleCount = r.SampleCount,
            XValuesJson = r.XValuesJson,
            YValuesJson = r.YValuesJson,
            ZValuesJson = r.ZValuesJson
        }).ToList();

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
