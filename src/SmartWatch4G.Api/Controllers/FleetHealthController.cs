using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Fleet health endpoints consumed by mobile and web applications.
/// Routes:
///   GET /api/fleet/health/latest        — most-recent health snapshot per device
///   GET /api/fleet/health/summary?date= — aggregated daily health stats per device
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("app-read")]
[ApiController]
[Route("api/v{version:apiVersion}/fleet")]
public sealed class FleetHealthController : ControllerBase
{
    private readonly IHealthDataRepository _healthRepo;
    private readonly ILogger<FleetHealthController> _logger;

    public FleetHealthController(
        IHealthDataRepository healthRepo,
        ILogger<FleetHealthController> logger)
    {
        _healthRepo = healthRepo;
        _logger = logger;
    }

    /// <summary>Returns the most recent health snapshot for every device.</summary>
    [HttpGet("health/latest")]
    public async Task<IActionResult> GetFleetHealthLatestAsync([FromQuery] string? tz, CancellationToken ct)
    {
        _logger.LogInformation("GetFleetHealthLatest — entry");
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        IReadOnlyList<HealthDataRecord> records;
        try
        {
            records = await _healthRepo.GetLatestAllDevicesAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFleetHealthLatest — DB read failed");
            return StatusCode(500, new ApiListResponse<HealthSnapshotDto> { ReturnCode = 500 });
        }

        var data = records.Select(r => MapToSnapshotDto(r, tzInfo)).ToList();
        _logger.LogInformation("GetFleetHealthLatest — exit, {Count} devices", data.Count);
        return Ok(new ApiListResponse<HealthSnapshotDto> { ReturnCode = 0, Count = data.Count, Data = data });
    }

    /// <summary>
    /// Returns aggregated daily health statistics (min/max/avg HR, avg SpO2, total steps, etc.)
    /// for every device that has data on the given date.
    /// </summary>
    [HttpGet("health/summary")]
    public async Task<IActionResult> GetFleetHealthSummaryAsync(
        [FromQuery] string date,
        CancellationToken ct)
    {
        _logger.LogInformation("GetFleetHealthSummary — entry, date: {Date}", date);

        if (!DateTimeUtilities.IsValidDate(date))
        {
            _logger.LogWarning("GetFleetHealthSummary — invalid date: {Date}", date);
            return BadRequest(new ApiListResponse<HealthDailyStatsDto> { ReturnCode = 400 });
        }

        IReadOnlyList<HealthDataRecord> records;
        try
        {
            records = await _healthRepo.GetAllDevicesAndDateAsync(date, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFleetHealthSummary — DB read failed for date {Date}", date);
            return StatusCode(500, new ApiListResponse<HealthDailyStatsDto> { ReturnCode = 500 });
        }

        var data = records
            .GroupBy(r => r.DeviceId)
            .Select(g => ComputeDailyStats(g.Key ?? string.Empty, date, g.ToList()))
            .OrderBy(s => s.DeviceId)
            .ToList();

        _logger.LogInformation(
            "GetFleetHealthSummary — exit, date: {Date}, devices: {Count}", date, data.Count);

        return Ok(new ApiListResponse<HealthDailyStatsDto>
        {
            ReturnCode = 0,
            Count = data.Count,
            Data = data
        });
    }

    private static HealthDailyStatsDto ComputeDailyStats(
        string deviceId,
        string date,
        IReadOnlyList<HealthDataRecord> records)
    {
        var hrValues    = records.Where(r => r.AvgHeartRate.HasValue).Select(r => r.AvgHeartRate!.Value).ToList();
        var maxHrVals   = records.Where(r => r.MaxHeartRate.HasValue).Select(r => r.MaxHeartRate!.Value).ToList();
        var minHrVals   = records.Where(r => r.MinHeartRate.HasValue).Select(r => r.MinHeartRate!.Value).ToList();
        var spo2Vals    = records.Where(r => r.AvgSpo2.HasValue).Select(r => r.AvgSpo2!.Value).ToList();
        var sbpVals     = records.Where(r => r.Sbp.HasValue).Select(r => r.Sbp!.Value).ToList();
        var dbpVals     = records.Where(r => r.Dbp.HasValue).Select(r => r.Dbp!.Value).ToList();
        var sdnnVals    = records.Where(r => r.HrvSdnn.HasValue).Select(r => r.HrvSdnn!.Value).ToList();
        var fatigueVals = records.Where(r => r.Fatigue.HasValue).Select(r => r.Fatigue!.Value).ToList();
        var tempVals    = records.Where(r => r.AxillaryTemp.HasValue).Select(r => r.AxillaryTemp!.Value).ToList();

        return new HealthDailyStatsDto
        {
            DeviceId            = deviceId,
            Date                = date,
            RecordCount         = records.Count,
            AvgHeartRate        = hrValues.Count    > 0 ? (long)Math.Round(hrValues.Average())    : null,
            MaxHeartRate        = maxHrVals.Count   > 0 ? maxHrVals.Max()                          : null,
            MinHeartRate        = minHrVals.Count   > 0 ? minHrVals.Min()                          : null,
            AvgSpo2             = spo2Vals.Count    > 0 ? (long)Math.Round(spo2Vals.Average())     : null,
            MinSpo2             = spo2Vals.Count    > 0 ? spo2Vals.Min()                           : null,
            TotalSteps          = records.Where(r => r.Steps.HasValue).Sum(r => r.Steps),
            TotalDistanceMetres = records.Where(r => r.DistanceMetres.HasValue).Sum(r => r.DistanceMetres),
            TotalCaloriesKcal   = records.Where(r => r.CaloriesKcal.HasValue).Sum(r => r.CaloriesKcal),
            AvgAxillaryTemp     = tempVals.Count    > 0 ? tempVals.Average()                        : null,
            AvgSbp              = sbpVals.Count     > 0 ? (long)Math.Round(sbpVals.Average())      : null,
            AvgDbp              = dbpVals.Count     > 0 ? (long)Math.Round(dbpVals.Average())      : null,
            AvgHrvSdnn          = sdnnVals.Count    > 0 ? Math.Round(sdnnVals.Average(), 2)        : null,
            AvgFatigue          = fatigueVals.Count > 0 ? (int)Math.Round(fatigueVals.Average())  : null
        };
    }

    private static HealthSnapshotDto MapToSnapshotDto(HealthDataRecord r, TimeZoneInfo? tz) => new()
    {
        DeviceId        = r.DeviceId ?? string.Empty,
        DataTime        = DateTimeUtilities.LocalizeTimestamp(r.DataTime, tz),
        Steps           = r.Steps,
        DistanceMetres  = r.DistanceMetres,
        CaloriesKcal    = r.CaloriesKcal,
        ActivityType    = r.ActivityType,
        ActivityState   = r.ActivityState,
        AvgHeartRate    = r.AvgHeartRate,
        MaxHeartRate    = r.MaxHeartRate,
        MinHeartRate    = r.MinHeartRate,
        AvgSpo2         = r.AvgSpo2,
        Sbp             = r.Sbp,
        Dbp             = r.Dbp,
        HrvSdnn         = r.HrvSdnn,
        HrvRmssd        = r.HrvRmssd,
        HrvPnn50        = r.HrvPnn50,
        HrvMean         = r.HrvMean,
        Fatigue         = r.Fatigue,
        AxillaryTemp    = r.AxillaryTemp,
        EstimatedTemp   = r.EstimatedTemp,
        BodyFat         = r.BodyFat,
        Bmi             = r.Bmi,
        BloodSugar      = r.BloodSugar,
        BloodPotassium  = r.BloodPotassium
    };
}
