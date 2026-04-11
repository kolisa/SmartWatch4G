using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Services;

/// <summary>
/// Infrastructure implementation of <see cref="IHealthQueryService"/>.
/// Moves repository access and DTO mapping out of the API layer.
/// Daily-stats aggregation logic lives here (single responsibility).
/// </summary>
public sealed class HealthQueryService : IHealthQueryService
{
    private readonly IHealthDataRepository _healthRepo;
    private readonly IDateTimeService _dt;

    public HealthQueryService(IHealthDataRepository healthRepo, IDateTimeService dt)
    {
        _healthRepo = healthRepo;
        _dt = dt;
    }

    public async Task<IReadOnlyList<HealthSnapshotDto>> GetSnapshotsByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = _dt.TryGetTimeZone(tz);
        var records = await _healthRepo.GetByDeviceAndDateAsync(deviceId, date, ct)
            .ConfigureAwait(false);
        return Map(records, tzInfo);
    }

    public async Task<IReadOnlyList<HealthSnapshotDto>> GetSnapshotsByRangeAsync(
        string deviceId, string from, string to, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = _dt.TryGetTimeZone(tz);
        var records = await _healthRepo.GetByDeviceAndTimeRangeAsync(deviceId, from, to, ct)
            .ConfigureAwait(false);
        return Map(records, tzInfo);
    }

    public async Task<HealthSnapshotDto?> GetLatestSnapshotAsync(
        string deviceId, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = _dt.TryGetTimeZone(tz);
        var r = await _healthRepo.GetLatestByDeviceAsync(deviceId, ct).ConfigureAwait(false);
        return r is null ? null : MapOne(r, tzInfo);
    }

    public async Task<HealthDailyStatsDto?> GetDailyStatsAsync(
        string deviceId, string date, CancellationToken ct = default)
    {
        var records = await _healthRepo.GetByDeviceAndDateAsync(deviceId, date, ct)
            .ConfigureAwait(false);
        if (records.Count == 0) return null;
        return ComputeDailyStats(deviceId, date, records);
    }

    public async Task<IReadOnlyList<HealthSnapshotDto>> GetLatestSnapshotAllDevicesAsync(
        string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = _dt.TryGetTimeZone(tz);
        var records = await _healthRepo.GetLatestAllDevicesAsync(ct).ConfigureAwait(false);
        return Map(records, tzInfo);
    }

    public async Task<IReadOnlyList<HealthDailyStatsDto>> GetDailyStatsAllDevicesAsync(
        string date, CancellationToken ct = default)
    {
        var records = await _healthRepo.GetAllDevicesAndDateAsync(date, ct).ConfigureAwait(false);

        return records
            .GroupBy(r => r.DeviceId)
            .Select(g => ComputeDailyStats(g.Key ?? string.Empty, date, g.ToList()))
            .OrderBy(s => s.DeviceId)
            .ToList();
    }

    // ── Mapping ───────────────────────────────────────────────────────────────

    private IReadOnlyList<HealthSnapshotDto> Map(
        IEnumerable<HealthDataRecord> records, TimeZoneInfo? tz)
        => records.Select(r => MapOne(r, tz)).ToList();

    private HealthSnapshotDto MapOne(HealthDataRecord r, TimeZoneInfo? tz) => new()
    {
        DeviceId = r.DeviceId ?? string.Empty,
        DataTime = _dt.LocalizeTimestamp(r.DataTime, tz),
        Steps = r.Steps,
        DistanceMetres = r.DistanceMetres,
        CaloriesKcal = r.CaloriesKcal,
        ActivityType = r.ActivityType,
        ActivityState = r.ActivityState,
        AvgHeartRate = r.AvgHeartRate,
        MaxHeartRate = r.MaxHeartRate,
        MinHeartRate = r.MinHeartRate,
        AvgSpo2 = r.AvgSpo2,
        Sbp = r.Sbp,
        Dbp = r.Dbp,
        HrvSdnn = r.HrvSdnn,
        HrvRmssd = r.HrvRmssd,
        HrvPnn50 = r.HrvPnn50,
        HrvMean = r.HrvMean,
        Fatigue = r.Fatigue,
        AxillaryTemp = r.AxillaryTemp,
        EstimatedTemp = r.EstimatedTemp,
        BodyFat = r.BodyFat,
        Bmi = r.Bmi,
        BloodSugar = r.BloodSugar,
        BloodPotassium = r.BloodPotassium
    };

    // ── Aggregation ───────────────────────────────────────────────────────────

    private static HealthDailyStatsDto ComputeDailyStats(
        string deviceId, string date, IReadOnlyList<HealthDataRecord> records)
    {
        var hrValues = records.Where(r => r.AvgHeartRate.HasValue).Select(r => r.AvgHeartRate!.Value).ToList();
        var maxHrVals = records.Where(r => r.MaxHeartRate.HasValue).Select(r => r.MaxHeartRate!.Value).ToList();
        var minHrVals = records.Where(r => r.MinHeartRate.HasValue).Select(r => r.MinHeartRate!.Value).ToList();
        var spo2Vals = records.Where(r => r.AvgSpo2.HasValue).Select(r => r.AvgSpo2!.Value).ToList();
        var sbpVals = records.Where(r => r.Sbp.HasValue).Select(r => r.Sbp!.Value).ToList();
        var dbpVals = records.Where(r => r.Dbp.HasValue).Select(r => r.Dbp!.Value).ToList();
        var sdnnVals = records.Where(r => r.HrvSdnn.HasValue).Select(r => r.HrvSdnn!.Value).ToList();
        var fatigueVals = records.Where(r => r.Fatigue.HasValue).Select(r => r.Fatigue!.Value).ToList();
        var tempVals = records.Where(r => r.AxillaryTemp.HasValue).Select(r => r.AxillaryTemp!.Value).ToList();

        return new HealthDailyStatsDto
        {
            DeviceId = deviceId,
            Date = date,
            RecordCount = records.Count,
            AvgHeartRate = hrValues.Count > 0 ? (long)Math.Round(hrValues.Average()) : null,
            MaxHeartRate = maxHrVals.Count > 0 ? maxHrVals.Max() : null,
            MinHeartRate = minHrVals.Count > 0 ? minHrVals.Min() : null,
            AvgSpo2 = spo2Vals.Count > 0 ? (long)Math.Round(spo2Vals.Average()) : null,
            MinSpo2 = spo2Vals.Count > 0 ? spo2Vals.Min() : null,
            TotalSteps = records.Where(r => r.Steps.HasValue).Sum(r => r.Steps),
            TotalDistanceMetres = records.Where(r => r.DistanceMetres.HasValue).Sum(r => r.DistanceMetres),
            TotalCaloriesKcal = records.Where(r => r.CaloriesKcal.HasValue).Sum(r => r.CaloriesKcal),
            AvgAxillaryTemp = tempVals.Count > 0 ? tempVals.Average() : null,
            AvgSbp = sbpVals.Count > 0 ? (long)Math.Round(sbpVals.Average()) : null,
            AvgDbp = dbpVals.Count > 0 ? (long)Math.Round(dbpVals.Average()) : null,
            AvgHrvSdnn = sdnnVals.Count > 0 ? Math.Round(sdnnVals.Average(), 2) : null,
            AvgFatigue = fatigueVals.Count > 0 ? (int)Math.Round(fatigueVals.Average()) : null
        };
    }
}
