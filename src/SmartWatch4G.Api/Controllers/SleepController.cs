using Microsoft.AspNetCore.RateLimiting;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Common;
using SmartWatch4G.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Returns computed sleep results for a given device and date.
/// Route: GET /health/sleep?deviceid=&amp;sleep_date=
/// </summary>
[EnableRateLimiting("device-write")]
[ApiController]
[Route("health/sleep")]
public sealed class SleepController : ControllerBase
{
    private readonly ISleepQueryService _sleepService;
    private readonly ILogger<SleepController> _logger;

    public SleepController(
        ISleepQueryService sleepService,
        ILogger<SleepController> logger)
    {
        _sleepService = sleepService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetSleepResultAsync(
        [FromQuery] string deviceid,
        [FromQuery] string sleep_date,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "GetSleepResult — entry, device: {DeviceId}, date: {Date}", deviceid, sleep_date);

        if (string.IsNullOrWhiteSpace(deviceid) || !DateTimeUtilities.IsValidDate(sleep_date))
        {
            _logger.LogWarning(
                "GetSleepResult — invalid parameters, device: {DeviceId}, date: {Date}",
                deviceid, sleep_date);
            return Ok(new ResponseCodeDto { ReturnCode = 10002 });
        }

        ServiceResult<SleepResult?> result;
        try
        {
            result = await _sleepService
                .GetSleepResultAsync(deviceid, sleep_date, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetSleepResult — service error for device {DeviceId}, date {Date}",
                deviceid, sleep_date);
            return Ok(new ResponseCodeDto { ReturnCode = 10002 });
        }

        if (result.IsFailure)
        {
            _logger.LogWarning(
                "GetSleepResult — service failure for device {DeviceId}, date {Date}: {Error}",
                deviceid, sleep_date, result.Error);
            return Ok(new ResponseCodeDto { ReturnCode = 10002 });
        }

        if (result.Value is null)
        {
            _logger.LogInformation(
                "GetSleepResult — no data found for device {DeviceId}, date {Date}",
                deviceid, sleep_date);
            return Ok(new ResponseCodeDto { ReturnCode = 10404 });
        }

        SleepResult r = result.Value;

        _logger.LogInformation(
            "GetSleepResult — exit, device: {DeviceId}, date: {Date}, score: {Score}",
            deviceid, sleep_date, r.Score);

        return Ok(new SleepResponseDto
        {
            ReturnCode = 0,
            Data = new SleepResultDto
            {
                DeviceId       = r.DeviceId,
                SleepDate      = r.SleepDate,
                StartTime      = r.StartTime,
                EndTime        = r.EndTime,
                DeepSleep      = r.DeepSleepMinutes,
                LightSleep     = r.LightSleepMinutes,
                WeakSleep      = r.WeakSleepMinutes,
                EyeMoveSleep   = r.EyeMoveSleepMinutes,
                Score          = r.Score,
                OsahsRisk      = r.OsahsRisk,
                Spo2Score      = r.Spo2Score,
                SleepHeartRate = r.SleepHeartRate
            }
        });
    }
}
