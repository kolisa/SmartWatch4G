using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

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
    private readonly IHealthDataRepository _healthRepo;
    private readonly ILogger<HealthDailyStatsController> _logger;

    public HealthDailyStatsController(
        IHealthDataRepository healthRepo,
        ILogger<HealthDailyStatsController> logger)
    {
        _healthRepo = healthRepo;
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

        IReadOnlyList<HealthDataRecord> records;
        try
        {
            records = await _healthRepo.GetByDeviceAndDateAsync(deviceId, date, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetDailyStats — DB read failed for device {DeviceId}, date {Date}",
                deviceId, date);
            return StatusCode(500, new ApiItemResponse<HealthDailyStatsDto> { ReturnCode = 500 });
        }

        if (records.Count == 0)
        {
            _logger.LogInformation(
                "GetDailyStats — no data for device {DeviceId}, date {Date}", deviceId, date);
            return NotFound(new ApiItemResponse<HealthDailyStatsDto> { ReturnCode = 404 });
        }

        HealthDailyStatsDto stats = ComputeDailyStats(deviceId, date, records);

        _logger.LogInformation(
            "GetDailyStats — exit, device: {DeviceId}, date: {Date}, records: {Count}",
            deviceId, date, records.Count);

        return Ok(new ApiItemResponse<HealthDailyStatsDto>
        {
            ReturnCode = 0,
            Data = stats
        });
    }

    private static HealthDailyStatsDto ComputeDailyStats(
        string deviceId,
        string date,
        IReadOnlyList<HealthDataRecord> records)
    {
        var hrValues  = records.Where(r => r.AvgHeartRate.HasValue).Select(r => r.AvgHeartRate!.Value).ToList();
        var maxHrVals = records.Where(r => r.MaxHeartRate.HasValue).Select(r => r.MaxHeartRate!.Value).ToList();
        var minHrVals = records.Where(r => r.MinHeartRate.HasValue).Select(r => r.MinHeartRate!.Value).ToList();
        var spo2Vals  = records.Where(r => r.AvgSpo2.HasValue).Select(r => r.AvgSpo2!.Value).ToList();
        var sbpVals   = records.Where(r => r.Sbp.HasValue).Select(r => r.Sbp!.Value).ToList();
        var dbpVals   = records.Where(r => r.Dbp.HasValue).Select(r => r.Dbp!.Value).ToList();
        var sdnnVals  = records.Where(r => r.HrvSdnn.HasValue).Select(r => r.HrvSdnn!.Value).ToList();
        var fatigueVals = records.Where(r => r.Fatigue.HasValue).Select(r => r.Fatigue!.Value).ToList();
        var tempVals  = records.Where(r => r.AxillaryTemp.HasValue).Select(r => r.AxillaryTemp!.Value).ToList();

        return new HealthDailyStatsDto
        {
            DeviceId          = deviceId,
            Date              = date,
            RecordCount       = records.Count,
            AvgHeartRate      = hrValues.Count  > 0 ? (long)Math.Round(hrValues.Average())  : null,
            MaxHeartRate      = maxHrVals.Count > 0 ? maxHrVals.Max()                        : null,
            MinHeartRate      = minHrVals.Count > 0 ? minHrVals.Min()                        : null,
            AvgSpo2           = spo2Vals.Count  > 0 ? (long)Math.Round(spo2Vals.Average())   : null,
            MinSpo2           = spo2Vals.Count  > 0 ? spo2Vals.Min()                         : null,
            TotalSteps        = records.Where(r => r.Steps.HasValue).Sum(r => r.Steps),
            TotalDistanceMetres = records.Where(r => r.DistanceMetres.HasValue).Sum(r => r.DistanceMetres),
            TotalCaloriesKcal = records.Where(r => r.CaloriesKcal.HasValue).Sum(r => r.CaloriesKcal),
            AvgAxillaryTemp   = tempVals.Count  > 0 ? tempVals.Average()                      : null,
            AvgSbp            = sbpVals.Count   > 0 ? (long)Math.Round(sbpVals.Average())    : null,
            AvgDbp            = dbpVals.Count   > 0 ? (long)Math.Round(dbpVals.Average())    : null,
            AvgHrvSdnn        = sdnnVals.Count  > 0 ? Math.Round(sdnnVals.Average(), 2)      : null,
            AvgFatigue        = fatigueVals.Count > 0 ? (int)Math.Round(fatigueVals.Average()) : null
        };
    }
}
