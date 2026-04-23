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
    private readonly ILogger<UserProfileQueryService> _logger;

    public UserProfileQueryService(IDatabaseService db, IDeviceStatusCache statusCache, ILogger<UserProfileQueryService> logger)
    {
        _db          = db;
        _statusCache = statusCache;
        _logger      = logger;
    }

    public async Task<ServiceResult<PagedResult<UserProfileSummaryResponse>>> GetPagedUserProfilesAsync(int page, int pageSize, int? companyId = null)
    {
        if (page < 1)    page     = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        try
        {
            var skip    = (page - 1) * pageSize;
            int total;
            IReadOnlyList<SmartWatch4G.Domain.Entities.UserProfile> workers;

            if (companyId.HasValue)
            {
                total   = _db.GetActiveWorkerCountByCompany(companyId.Value);
                workers = _db.GetPagedUserProfilesByCompany(skip, pageSize, companyId.Value);
            }
            else
            {
                total   = _db.GetActiveWorkerCount();
                workers = _db.GetPagedUserProfiles(skip, pageSize);
            }

            // Parallel fetch of latest health + GPS for each user profile on this page
            var tasks = workers.Select(async w =>
            {
                var health = await Task.Run(() => _db.GetLatestHealthSnapshot(w.DeviceId));
                var track  = await Task.Run(() => _db.GetLatestGnssTrack(w.DeviceId));
                return MapSummary(w, health, track);
            });

            var items = await Task.WhenAll(tasks);

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
            var profile = _db.GetUserProfile(deviceId);
            if (profile is null)
                return ServiceResult<UserProfileDetailResponse>.Fail("User profile not found.", 404);

            var healthTask = Task.Run(() => _db.GetLatestHealthSnapshot(deviceId));
            var trackTask  = Task.Run(() => _db.GetLatestGnssTrack(deviceId));

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

    private static UserProfileSummaryResponse MapSummary(
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

    public async Task<ServiceResult<DeviceTelemetryResponse>> GetDeviceTelemetryAsync(string deviceId)
    {
        try
        {
            var profile = _db.GetUserProfile(deviceId);
            if (profile is null)
                return ServiceResult<DeviceTelemetryResponse>.Fail("Device not found.", 404);

            var healthTask = Task.Run(() => _db.GetLatestHealthSnapshot(deviceId));
            var trackTask  = Task.Run(() => _db.GetLatestGnssTrack(deviceId));
            await Task.WhenAll(healthTask, trackTask);

            var health = healthTask.Result;
            var track  = trackTask.Result;

            return ServiceResult<DeviceTelemetryResponse>.Ok(MapTelemetry(deviceId, health, track, _statusCache.GetStatus(deviceId)));
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
                ? _db.GetUsersByCompanyId(companyId.Value)
                : _db.GetAllUserProfiles();

            var tasks = profiles.Select(async p =>
            {
                var health = await Task.Run(() => _db.GetLatestHealthSnapshot(p.DeviceId));
                var track  = await Task.Run(() => _db.GetLatestGnssTrack(p.DeviceId));
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
