using Microsoft.Extensions.Logging;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Common;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces;

namespace SmartWatch4G.Infrastructure.Services;

public sealed class WorkerQueryService : IWorkerQueryService
{
    private readonly IDatabaseService _db;
    private readonly IDeviceStatusCache _statusCache;
    private readonly ILogger<WorkerQueryService> _logger;

    public WorkerQueryService(IDatabaseService db, IDeviceStatusCache statusCache, ILogger<WorkerQueryService> logger)
    {
        _db          = db;
        _statusCache = statusCache;
        _logger      = logger;
    }

    public async Task<ServiceResult<PagedResult<WorkerSummaryResponse>>> GetPagedWorkersAsync(int page, int pageSize)
    {
        if (page < 1)    page     = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        try
        {
            var total   = _db.GetActiveWorkerCount();
            var skip    = (page - 1) * pageSize;
            var workers = _db.GetPagedUserProfiles(skip, pageSize);

            // Parallel fetch of latest health + GPS for each worker on this page
            var tasks = workers.Select(async w =>
            {
                var health = await Task.Run(() => _db.GetLatestHealthSnapshot(w.DeviceId));
                var track  = await Task.Run(() => _db.GetLatestGnssTrack(w.DeviceId));
                return MapSummary(w, health, track);
            });

            var items = await Task.WhenAll(tasks);

            return ServiceResult<PagedResult<WorkerSummaryResponse>>.Ok(new PagedResult<WorkerSummaryResponse>
            {
                Items      = items,
                TotalCount = total,
                Page       = page,
                PageSize   = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPagedWorkersAsync failed");
            return ServiceResult<PagedResult<WorkerSummaryResponse>>.Fail("An unexpected error occurred.", 500);
        }
    }

    public async Task<ServiceResult<WorkerDetailResponse>> GetWorkerDetailAsync(string deviceId)
    {
        try
        {
            var profile = _db.GetUserProfile(deviceId);
            if (profile is null)
                return ServiceResult<WorkerDetailResponse>.Fail("Worker not found.", 404);

            var healthTask = Task.Run(() => _db.GetLatestHealthSnapshot(deviceId));
            var trackTask  = Task.Run(() => _db.GetLatestGnssTrack(deviceId));

            await Task.WhenAll(healthTask, trackTask);

            var health = healthTask.Result;
            var track  = trackTask.Result;
            var status = _statusCache.GetStatus(deviceId);

            return ServiceResult<WorkerDetailResponse>.Ok(new WorkerDetailResponse
            {
                DeviceId          = profile.DeviceId,
                Name              = profile.Name,
                Surname           = profile.Surname,
                EmpNo             = profile.EmpNo,
                Email             = profile.Email,
                Cell              = profile.Cell,
                Address           = profile.Address,
                DeviceStatus      = status,
                LatestLatitude    = track?.Latitude,
                LatestLongitude   = track?.Longitude,
                LatestGnssTime    = track?.GnssTime,
                SpO2              = health?.AvgSpo2,
                Steps             = health?.Steps,
                HeartRate         = health?.AvgHr,
                MaxHeartRate      = health?.MaxHr,
                MinHeartRate      = health?.MinHr,
                Fatigue           = health?.Fatigue,
                Battery           = health?.Battery,
                Sbp               = health?.Sbp,
                Dbp               = health?.Dbp,
                Distance          = health?.Distance,
                Calorie           = health?.Calorie,
                HealthRecordedAt  = health?.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetWorkerDetailAsync failed for {DeviceId}", deviceId);
            return ServiceResult<WorkerDetailResponse>.Fail("An unexpected error occurred.", 500);
        }
    }

    private static WorkerSummaryResponse MapSummary(
        UserProfile user, HealthSnapshot? health, GnssTrack? track) => new()
    {
        DeviceId          = user.DeviceId,
        Name              = user.Name,
        Surname           = user.Surname,
        EmpNo             = user.EmpNo,
        LatestLatitude    = track?.Latitude,
        LatestLongitude   = track?.Longitude,
        LatestGnssTime    = track?.GnssTime,
        SpO2              = health?.AvgSpo2,
        Steps             = health?.Steps,
        HeartRate         = health?.AvgHr,
        Fatigue           = health?.Fatigue,
        Battery           = health?.Battery,
        Sbp               = health?.Sbp,
        Dbp               = health?.Dbp,
        HealthRecordedAt  = health?.CreatedAt
    };
}
