using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Interfaces;

namespace KhoiWatchData.Api.Controllers;

[ApiVersionNeutral]
[ApiController]
[Route("health/sleep")]
public class SleepController : ControllerBase
{
    private readonly ILogger<SleepController> _logger;
    private readonly IDatabaseService _db;

    public SleepController(ILogger<SleepController> logger, IDatabaseService db)
    {
        _logger = logger;
        _db     = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetSleepResult([FromQuery] string deviceid, [FromQuery] string sleep_date)
    {
        _logger.LogInformation("GetSleepResult request: deviceid={DeviceId} sleep_date={SleepDate}",
            deviceid, sleep_date);

        if (string.IsNullOrEmpty(deviceid) || !DateTimeUtilities.IsValidDate(sleep_date))
        {
            _logger.LogWarning("GetSleepResult bad params: deviceid={DeviceId} sleep_date={SleepDate}",
                deviceid, sleep_date);
            return Ok(new { ReturnCode = 10002 });
        }

        var record = await _db.GetSleepCalculation(deviceid, sleep_date);

        _logger.LogInformation("GetSleepResult DB result for {DeviceId} {SleepDate}: {Found}",
            deviceid, sleep_date, record is not null ? "found" : "not found");

        if (record is null)
            return Ok(new { ReturnCode = 10404 });

        string prevDay = DateTimeUtilities.GetPreviousDay(sleep_date);

        return Ok(new
        {
            ReturnCode = 0,
            Data = new
            {
                deviceid     = record.DeviceId,
                sleep_date   = record.RecordDate,
                start_time   = record.StartTime ?? $"{prevDay} 00:00:00",
                end_time     = record.EndTime   ?? $"{sleep_date} 00:00:00",
                deep_sleep   = record.DeepSleep   ?? 0,
                light_sleep  = record.LightSleep  ?? 0,
                weak_sleep   = record.WeakSleep   ?? 0,
                eyemove_sleep = record.EyemoveSleep ?? 0,
                score        = 0,
                osahs_risk   = 0,
                spo2_score   = 0,
                sleep_hr     = record.Hr
            }
        });
    }
}
