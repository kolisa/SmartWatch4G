using Microsoft.Extensions.Logging;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Common;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces;

namespace SmartWatch4G.Infrastructure.Services;

public sealed class UserProfileQueryService : IUserProfileQueryService
{
    private const string UnexpectedError = "An unexpected error occurred.";

    private readonly IDatabaseService _db;
    private readonly IDeviceStatusCache _statusCache;
    private readonly IwownService _iwown;
    private readonly ILogger<UserProfileQueryService> _logger;

    public UserProfileQueryService(IDatabaseService db, IDeviceStatusCache statusCache,
        IwownService iwown, ILogger<UserProfileQueryService> logger)
    {
        _db          = db;
        _statusCache = statusCache;
        _iwown       = iwown;
        _logger      = logger;
    }

    public async Task<ServiceResult<PagedResult<UserProfileSummaryResponse>>> GetPagedUserProfilesAsync(int page, int pageSize, int? companyId = null)
    {
        if (page < 1)     page     = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        try
        {
            var skip = (page - 1) * pageSize;
            int total;
            IReadOnlyList<UserProfile> workers;

            if (companyId.HasValue)
            {
                total   = await _db.GetActiveWorkerCountByCompany(companyId.Value);
                workers = await _db.GetPagedUserProfilesByCompany(skip, pageSize, companyId.Value);
            }
            else
            {
                total   = await _db.GetActiveWorkerCount();
                workers = await _db.GetPagedUserProfiles(skip, pageSize);
            }

            // Fetch health, GPS, and Iwown status in parallel across all devices on the page
            var statusTasks = workers.Select(w => _iwown.GetDeviceStatusAsync(w.DeviceId)).ToArray();
            var dataTask = Task.WhenAll(workers.Select(async w =>
            {
                var health = await _db.GetLatestHealthSnapshot(w.DeviceId);
                var track  = await _db.GetLatestGnssTrack(w.DeviceId);
                return (health, track);
            }));
            await Task.WhenAll(Task.WhenAll(statusTasks), dataTask);

            var data  = dataTask.Result;
            var items = workers.Select((w, i) =>
            {
                var isOnline = DeviceStatusParser.IsOnline(statusTasks[i].Result);
                _statusCache.SetStatus(w.DeviceId, isOnline);
                return MapSummary(w, data[i].health, data[i].track, isOnline);
            })
            .OrderByDescending(x => x.StatusCode)
            .ThenBy(x => x.Name)
            .ThenBy(x => x.Surname)
            .ToArray();

            return ServiceResult<PagedResult<UserProfileSummaryResponse>>.Ok(new PagedResult<UserProfileSummaryResponse>
            {
                Items      = items,
                TotalCount = total,
                Page       = page,
                PageSize   = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPagedUserProfilesAsync failed");
            return ServiceResult<PagedResult<UserProfileSummaryResponse>>.Fail(UnexpectedError, 500);
        }
    }

    public async Task<ServiceResult<UserProfileDetailResponse>> GetUserProfileDetailAsync(string deviceId)
    {
        try
        {
            var profile = await _db.GetUserProfile(deviceId);
            if (profile is null)
                return ServiceResult<UserProfileDetailResponse>.Fail("User profile not found.", 404);

            var healthTask = _db.GetLatestHealthSnapshot(deviceId);
            var trackTask  = _db.GetLatestGnssTrack(deviceId);

            await Task.WhenAll(healthTask, trackTask);

            var health = healthTask.Result;
            var track  = trackTask.Result;
            var status = _statusCache.GetStatus(deviceId);

            return ServiceResult<UserProfileDetailResponse>.Ok(new UserProfileDetailResponse
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
            _logger.LogError(ex, "GetUserProfileDetailAsync failed for {DeviceId}", deviceId);
            return ServiceResult<UserProfileDetailResponse>.Fail(UnexpectedError, 500);
        }
    }

    public async Task<ServiceResult<DeviceStatusPagedResult>> GetDeviceStatusPagedAsync(int page, int pageSize, int? companyId = null)
    {
        if (page < 1)        page     = 1;
        if (pageSize < 1)    pageSize = 1000;
        if (pageSize > 1000) pageSize = 1000;

        try
        {
            var skip = (page - 1) * pageSize;
            int total;
            IReadOnlyList<UserProfile> workers;

            if (companyId.HasValue)
            {
                total   = await _db.GetActiveWorkerCountByCompany(companyId.Value);
                workers = await _db.GetPagedUserProfilesByCompany(skip, pageSize, companyId.Value);
            }
            else
            {
                total   = await _db.GetActiveWorkerCount();
                workers = await _db.GetPagedUserProfiles(skip, pageSize);
            }

            // Fetch Iwown status and latest GPS in parallel for every device on this page
            var statusTasks = workers.Select(w => _iwown.GetDeviceStatusAsync(w.DeviceId)).ToArray();
            var gpsTasks    = workers.Select(w => _db.GetLatestGnssTrack(w.DeviceId)).ToArray();
            await Task.WhenAll(Task.WhenAll(statusTasks), Task.WhenAll(gpsTasks));

            var items = workers.Select((w, i) =>
            {
                var isOnline = DeviceStatusParser.IsOnline(statusTasks[i].Result);
                var track    = gpsTasks[i].Result;
                _statusCache.SetStatus(w.DeviceId, isOnline);
                return new DeviceStatusItem
                {
                    DeviceId        = w.DeviceId,
                    Name            = w.Name,
                    Surname         = w.Surname,
                    EmpNo           = w.EmpNo,
                    Status          = isOnline ? "online" : "offline",
                    StatusCode      = isOnline ? 1 : 0,
                    LatestLatitude  = track?.Latitude,
                    LatestLongitude = track?.Longitude,
                    LatestGnssTime  = track?.GnssTime
                };
            }).ToList();

            var onlineCount  = items.Count(x => x.StatusCode == 1);
            var offlineCount = items.Count(x => x.StatusCode == 0);

            return ServiceResult<DeviceStatusPagedResult>.Ok(new DeviceStatusPagedResult
            {
                Items        = items,
                TotalCount   = total,
                Page         = page,
                PageSize     = pageSize,
                OnlineCount  = onlineCount,
                OfflineCount = offlineCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDeviceStatusPagedAsync failed");
            return ServiceResult<DeviceStatusPagedResult>.Fail(UnexpectedError, 500);
        }
    }

    private static UserProfileSummaryResponse MapSummary(
        UserProfile user, HealthSnapshot? health, GnssTrack? track, bool isOnline) => new()
    {
        DeviceId          = user.DeviceId,
        Name              = user.Name,
        Surname           = user.Surname,
        EmpNo             = user.EmpNo,
        Status            = isOnline ? "online" : "offline",
        StatusCode        = isOnline ? 1 : 0,
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

    public async Task<ServiceResult<DeviceTelemetryResponse>> GetDeviceTelemetryAsync(string deviceId)
    {
        try
        {
            var profile = await _db.GetUserProfile(deviceId);
            if (profile is null)
                return ServiceResult<DeviceTelemetryResponse>.Fail("Device not found.", 404);

            var healthTask = _db.GetLatestHealthSnapshot(deviceId);
            var trackTask  = _db.GetLatestGnssTrack(deviceId);
            await Task.WhenAll(healthTask, trackTask);

            return ServiceResult<DeviceTelemetryResponse>.Ok(
                MapTelemetry(deviceId, healthTask.Result, trackTask.Result, _statusCache.GetStatus(deviceId)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDeviceTelemetryAsync failed for {DeviceId}", deviceId);
            return ServiceResult<DeviceTelemetryResponse>.Fail(UnexpectedError, 500);
        }
    }

    public async Task<ServiceResult<IReadOnlyList<DeviceTelemetryResponse>>> GetAllDeviceTelemetryAsync(int? companyId = null)
    {
        try
        {
            var profiles = companyId.HasValue
                ? await _db.GetUsersByCompanyId(companyId.Value)
                : await _db.GetAllUserProfiles();

            var tasks = profiles.Select(async p =>
            {
                var health = await _db.GetLatestHealthSnapshot(p.DeviceId);
                var track  = await _db.GetLatestGnssTrack(p.DeviceId);
                return MapTelemetry(p.DeviceId, health, track, _statusCache.GetStatus(p.DeviceId));
            });

            var items = await Task.WhenAll(tasks);

            return ServiceResult<IReadOnlyList<DeviceTelemetryResponse>>.Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllDeviceTelemetryAsync failed");
            return ServiceResult<IReadOnlyList<DeviceTelemetryResponse>>.Fail(UnexpectedError, 500);
        }
    }

    private static DeviceTelemetryResponse MapTelemetry(
        string deviceId, HealthSnapshot? health, GnssTrack? track, string status) => new()
    {
        DeviceId         = deviceId,
        DeviceStatus     = status,
        Battery          = health?.Battery,
        HeartRate        = health?.AvgHr,
        SpO2             = health?.AvgSpo2,
        Fatigue          = health?.Fatigue,
        Sbp              = health?.Sbp,
        Dbp              = health?.Dbp,
        Steps            = health?.Steps,
        Latitude         = track?.Latitude,
        Longitude        = track?.Longitude,
        GnssTime         = track?.GnssTime,
        HealthRecordedAt = health?.CreatedAt
    };
}
