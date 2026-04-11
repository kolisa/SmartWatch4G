using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Common;
using SmartWatch4G.Domain.Interfaces.Services;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Read-only sleep analysis endpoint consumed by mobile and web applications.
/// Route: GET /api/devices/{deviceId}/sleep?date=
///
/// Note: the device-facing legacy sleep endpoint remains at GET /health/sleep
/// (see <see cref="SleepController"/>).
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("dashboard-api")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/sleep")]
public sealed class SleepAnalysisController : ControllerBase
{
    private readonly ISleepQueryService _sleepService;
    private readonly ILogger<SleepAnalysisController> _logger;

    public SleepAnalysisController(
        ISleepQueryService sleepService,
        ILogger<SleepAnalysisController> logger)
    {
        _sleepService = sleepService;
        _logger = logger;
    }

    /// <summary>Returns the computed sleep analysis result for the given device and date.</summary>
    [HttpGet]
    public async Task<IActionResult> GetSleepAsync(
        string deviceId,
        [FromQuery] string date,
        [FromQuery] string? tz,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "GetSleepAnalysis — entry, device: {DeviceId}, date: {Date}", deviceId, date);

        if (string.IsNullOrWhiteSpace(deviceId) || !DateTimeUtilities.IsValidDate(date))
        {
            _logger.LogWarning(
                "GetSleepAnalysis — invalid parameters, device: {DeviceId}, date: {Date}",
                deviceId, date);
            return BadRequest(new ApiItemResponse<SleepResultDto> { ReturnCode = 400 });
        }

        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        ServiceResult<SleepResult?> result;
        try
        {
            result = await _sleepService.GetSleepResultAsync(deviceId, date, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetSleepAnalysis — service error for device {DeviceId}, date {Date}",
                deviceId, date);
            return StatusCode(500, new ApiItemResponse<SleepResultDto> { ReturnCode = 500 });
        }

        if (result.IsFailure)
        {
            _logger.LogWarning(
                "GetSleepAnalysis — failure for device {DeviceId}, date {Date}: {Error}",
                deviceId, date, result.Error);
            return StatusCode(result.ErrorCode == 400 ? 400 : 500,
                new ApiItemResponse<SleepResultDto> { ReturnCode = result.ErrorCode });
        }

        if (result.Value is null)
        {
            _logger.LogInformation(
                "GetSleepAnalysis — no data for device {DeviceId}, date {Date}", deviceId, date);
            return NotFound(new ApiItemResponse<SleepResultDto> { ReturnCode = 404 });
        }

        SleepResult r = result.Value;

        _logger.LogInformation(
            "GetSleepAnalysis — exit, device: {DeviceId}, date: {Date}, score: {Score}",
            deviceId, date, r.Score);

        return Ok(new ApiItemResponse<SleepResultDto>
        {
            ReturnCode = 0,
            Data = new SleepResultDto
            {
                DeviceId = r.DeviceId,
                SleepDate = r.SleepDate,
                StartTime = DateTimeUtilities.LocalizeTimestamp(r.StartTime, tzInfo),
                EndTime = DateTimeUtilities.LocalizeTimestamp(r.EndTime, tzInfo),
                DeepSleep = r.DeepSleepMinutes,
                LightSleep = r.LightSleepMinutes,
                WeakSleep = r.WeakSleepMinutes,
                EyeMoveSleep = r.EyeMoveSleepMinutes,
                Score = r.Score,
                OsahsRisk = r.OsahsRisk,
                Spo2Score = r.Spo2Score,
                SleepHeartRate = r.SleepHeartRate
            }
        });
    }
}
