using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Common;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using SmartWatch4G.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Fleet sleep endpoint consumed by mobile and web applications.
/// Route: GET /api/fleet/sleep?date=
/// Returns computed sleep results for every device that has data on the given date.
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("app-read")]
[ApiController]
[Route("api/v{version:apiVersion}/fleet")]
public sealed class FleetSleepController : ControllerBase
{
    private readonly IDeviceInfoRepository _deviceInfoRepo;
    private readonly ISleepQueryService _sleepService;
    private readonly ILogger<FleetSleepController> _logger;

    public FleetSleepController(
        IDeviceInfoRepository deviceInfoRepo,
        ISleepQueryService sleepService,
        ILogger<FleetSleepController> logger)
    {
        _deviceInfoRepo = deviceInfoRepo;
        _sleepService = sleepService;
        _logger = logger;
    }

    /// <summary>
    /// Returns computed sleep results for all devices on the given date.
    /// Devices with no sleep data for that date are omitted.
    /// </summary>
    [HttpGet("sleep")]
    public async Task<IActionResult> GetFleetSleepAsync(
        [FromQuery] string date,
        [FromQuery] string? tz,
        CancellationToken ct)
    {
        _logger.LogInformation("GetFleetSleep — entry, date: {Date}", date);

        if (!DateTimeUtilities.IsValidDate(date))
        {
            _logger.LogWarning("GetFleetSleep — invalid date: {Date}", date);
            return BadRequest(new ApiListResponse<SleepResultDto> { ReturnCode = 400 });
        }

        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        IReadOnlyList<DeviceInfoRecord> devices;
        try
        {
            devices = await _deviceInfoRepo.GetAllAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFleetSleep — failed to load device list");
            return StatusCode(500, new ApiListResponse<SleepResultDto> { ReturnCode = 500 });
        }

        var results = new List<SleepResultDto>();
        foreach (string deviceId in devices.Select(d => d.DeviceId))
        {
            try
            {
                ServiceResult<SleepResult?> sr = await _sleepService
                    .GetSleepResultAsync(deviceId, date, ct)
                    .ConfigureAwait(false);

                if (sr.IsSuccess && sr.Value is not null)
                {
                    SleepResult r = sr.Value;
                    results.Add(new SleepResultDto
                    {
                        DeviceId       = r.DeviceId,
                        SleepDate      = r.SleepDate,
                        StartTime      = DateTimeUtilities.LocalizeTimestamp(r.StartTime, tzInfo),
                        EndTime        = DateTimeUtilities.LocalizeTimestamp(r.EndTime, tzInfo),
                        DeepSleep      = r.DeepSleepMinutes,
                        LightSleep     = r.LightSleepMinutes,
                        WeakSleep      = r.WeakSleepMinutes,
                        EyeMoveSleep   = r.EyeMoveSleepMinutes,
                        Score          = r.Score,
                        OsahsRisk      = r.OsahsRisk,
                        Spo2Score      = r.Spo2Score,
                        SleepHeartRate = r.SleepHeartRate
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "GetFleetSleep — sleep service error for device {DeviceId}, date {Date}",
                    deviceId, date);
            }
        }

        _logger.LogInformation(
            "GetFleetSleep — exit, date: {Date}, devices with data: {Count}", date, results.Count);

        return Ok(new ApiListResponse<SleepResultDto>
        {
            ReturnCode = 0,
            Count = results.Count,
            Data = results
        });
    }
}
