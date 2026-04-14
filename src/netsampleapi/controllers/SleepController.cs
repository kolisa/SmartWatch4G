using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using SampleApi.Utils;

namespace SampleApi.Controller  {
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
            // Validate inputs
            if (string.IsNullOrEmpty(deviceid) || !MyDateTimeUtils.IsValidDate(sleep_date))
            {
                return Ok(new
                {
                    ReturnCode = 10002
                });
            }

            _logger.LogInformation($"getSleepResult {deviceid} {sleep_date}");

            // Simulate data availability check
            bool dataExist = true; // Replace with actual logic to check data availability

            if (dataExist)
            {
                string prevDay = MyDateTimeUtils.GetPreviousDay(sleep_date);

                // Build the sleep data response
                var sleepData = new
                {
                    deviceid = deviceid,
                    sleep_date = sleep_date,
                    start_time = $"{prevDay} 23:15:00",
                    end_time = $"{sleep_date} 07:00:00",
                    deep_sleep = 85,
                    light_sleep = 300,
                    weak_sleep = 30,
                    eyemove_sleep = 50,
                    score = 80,
                    osahs_risk = 0,
                    spo2_score = 0,
                    sleep_hr = 60
                };

                return Ok(new
                {
                    ReturnCode = 0,
                    Data = sleepData
                });
            }
            else
            {
                return Ok(new
                {
                    ReturnCode = 10404
                });
            }
        }
    }
}
