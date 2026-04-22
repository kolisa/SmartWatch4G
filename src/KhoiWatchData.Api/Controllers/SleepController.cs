using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.Utilities;

namespace KhoiWatchData.Api.Controllers;

[ApiController]
[Route("health/sleep")]
public class SleepController : ControllerBase
{
    private readonly ILogger<SleepController> _logger;

    public SleepController(ILogger<SleepController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetSleepResult([FromQuery] string deviceid, [FromQuery] string sleep_date)
    {
        if (string.IsNullOrEmpty(deviceid) || !DateTimeUtilities.IsValidDate(sleep_date))
            return Ok(new { ReturnCode = 10002 });

        _logger.LogInformation("getSleepResult {DeviceId} {SleepDate}", deviceid, sleep_date);

        string prevDay = DateTimeUtilities.GetPreviousDay(sleep_date);

        var sleepData = new
        {
            deviceid,
            sleep_date,
            start_time   = $"{prevDay} 23:15:00",
            end_time     = $"{sleep_date} 07:00:00",
            deep_sleep   = 85,
            light_sleep  = 300,
            weak_sleep   = 30,
            eyemove_sleep = 50,
            score        = 80,
            osahs_risk   = 0,
            spo2_score   = 0,
            sleep_hr     = 60
        };

        return Ok(new { ReturnCode = 0, Data = sleepData });
    }
}
