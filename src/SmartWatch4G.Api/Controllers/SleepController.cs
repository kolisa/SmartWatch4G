using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Returns computed sleep results for a given device and date.
/// Route: GET /health/sleep?deviceid=&amp;sleep_date=
/// </summary>
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
        if (string.IsNullOrWhiteSpace(deviceid) || !DateTimeUtilities.IsValidDate(sleep_date))
        {
            return Ok(new ResponseCodeDto { ReturnCode = 10002 });
        }

        _logger.LogInformation("GetSleepResult — device: {DeviceId}, date: {Date}", deviceid, sleep_date);

        SleepResult? result = await _sleepService
            .GetSleepResultAsync(deviceid, sleep_date, ct)
            .ConfigureAwait(false);

        if (result is null)
        {
            return Ok(new ResponseCodeDto { ReturnCode = 10404 });
        }

        return Ok(new SleepResponseDto
        {
            ReturnCode = 0,
            Data = new SleepResultDto
            {
                DeviceId = result.DeviceId,
                SleepDate = result.SleepDate,
                StartTime = result.StartTime,
                EndTime = result.EndTime,
                DeepSleep = result.DeepSleepMinutes,
                LightSleep = result.LightSleepMinutes,
                WeakSleep = result.WeakSleepMinutes,
                EyeMoveSleep = result.EyeMoveSleepMinutes,
                Score = result.Score,
                OsahsRisk = result.OsahsRisk,
                Spo2Score = result.Spo2Score,
                SleepHeartRate = result.SleepHeartRate
            }
        });
    }
}
