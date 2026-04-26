using Microsoft.Extensions.Logging;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Common;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces;

namespace SmartWatch4G.Infrastructure.Services;

public sealed class HealthQueryService : IHealthQueryService
{
    private const string UnexpectedError = "An unexpected error occurred.";

    private readonly IDatabaseService _db;
    private readonly ILogger<HealthQueryService> _logger;

    public HealthQueryService(IDatabaseService db, ILogger<HealthQueryService> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task<ServiceResult<HealthPagedResult>> GetByCompanyAsync(int companyId, HealthQueryParams q)
    {
        try
        {
            if (!ValidateDateRange(q.From, q.To, out var err))
                return ServiceResult<HealthPagedResult>.Fail(err!, 400);

            var (skip, take) = Paging(q);
            var (items, total) = await _db.GetHealthSnapshotsByCompany(
                companyId, q.From, q.To, skip, take, q.SortDir);

            var result = new HealthPagedResult
            {
                Items      = items.Select(x => MapRecord(x.DeviceId, x.UserName, x.Snapshot)).ToList(),
                TotalCount = total,
                Page       = q.Page,
                PageSize   = q.PageSize
            };
            return ServiceResult<HealthPagedResult>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HealthQueryService.GetByCompanyAsync failed for company {Id}", companyId);
            return ServiceResult<HealthPagedResult>.Fail(UnexpectedError, 500);
        }
    }

    public async Task<ServiceResult<IReadOnlyList<HealthSummaryResponse>>> GetSummaryByCompanyAsync(
        int companyId, System.DateTime? from, System.DateTime? to)
    {
        try
        {
            if (!ValidateDateRange(from, to, out var err))
                return ServiceResult<IReadOnlyList<HealthSummaryResponse>>.Fail(err!, 400);

            var rows = await _db.GetHealthSummaryByCompany(companyId, from, to);
            IReadOnlyList<HealthSummaryResponse> list = rows.Select(x => new HealthSummaryResponse
            {
                DeviceId     = x.DeviceId,
                UserName     = x.UserName,
                AvgHeartRate = x.AvgHr.HasValue  ? Math.Round(x.AvgHr.Value,  1) : null,
                AvgSpO2      = x.AvgSpo2.HasValue ? Math.Round(x.AvgSpo2.Value, 1) : null,
                AvgFatigue   = x.AvgFatigue.HasValue ? Math.Round(x.AvgFatigue.Value, 1) : null,
                MaxHr        = x.MaxHr,
                MinHr        = x.MinHr,
                TotalSteps   = x.TotalSteps,
                RecordCount  = x.Count
            }).ToList();

            return ServiceResult<IReadOnlyList<HealthSummaryResponse>>.Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HealthQueryService.GetSummaryByCompanyAsync failed for company {Id}", companyId);
            return ServiceResult<IReadOnlyList<HealthSummaryResponse>>.Fail(UnexpectedError, 500);
        }
    }

    public async Task<ServiceResult<HealthPagedResult>> GetByDeviceAsync(string deviceId, HealthQueryParams q)
    {
        try
        {
            if (!ValidateDateRange(q.From, q.To, out var err))
                return ServiceResult<HealthPagedResult>.Fail(err!, 400);

            var (skip, take) = Paging(q);
            var (items, total) = await _db.GetHealthSnapshotsByDevice(
                deviceId, q.From, q.To, skip, take, q.SortDir);

            var result = new HealthPagedResult
            {
                Items      = items.Select(s => MapRecord(deviceId, null, s)).ToList(),
                TotalCount = total,
                Page       = q.Page,
                PageSize   = q.PageSize
            };
            return ServiceResult<HealthPagedResult>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HealthQueryService.GetByDeviceAsync failed for {Device}", deviceId);
            return ServiceResult<HealthPagedResult>.Fail(UnexpectedError, 500);
        }
    }

    public async Task<ServiceResult<HealthRecordResponse>> GetLatestByDeviceAsync(string deviceId)
    {
        try
        {
            var snap = await _db.GetLatestHealthSnapshot(deviceId);
            if (snap is null)
                return ServiceResult<HealthRecordResponse>.Fail("No health data found for this device.", 404);

            return ServiceResult<HealthRecordResponse>.Ok(MapRecord(deviceId, null, snap));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HealthQueryService.GetLatestByDeviceAsync failed for {Device}", deviceId);
            return ServiceResult<HealthRecordResponse>.Fail(UnexpectedError, 500);
        }
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static (int skip, int take) Paging(HealthQueryParams q)
    {
        var page     = Math.Max(1, q.Page);
        var pageSize = Math.Clamp(q.PageSize, 1, 200);
        return ((page - 1) * pageSize, pageSize);
    }

    private static bool ValidateDateRange(System.DateTime? from, System.DateTime? to, out string? error)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        { error = "'from' must be earlier than or equal to 'to'."; return false; }
        error = null;
        return true;
    }

    private static HealthRecordResponse MapRecord(string deviceId, string? userName, HealthSnapshot s) => new()
    {
        DeviceId   = deviceId,
        UserName   = userName,
        RecordTime = s.RecordTime,
        Battery    = s.Battery,
        Steps      = s.Steps,
        Distance   = s.Distance,
        Calorie    = s.Calorie,
        HeartRate  = s.AvgHr,
        MaxHr      = s.MaxHr,
        MinHr      = s.MinHr,
        SpO2       = s.AvgSpo2,
        Sbp        = s.Sbp,
        Dbp        = s.Dbp,
        Fatigue    = s.Fatigue,
        RecordedAt = s.CreatedAt
    };
}
