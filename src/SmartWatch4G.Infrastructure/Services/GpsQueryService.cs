using Microsoft.Extensions.Logging;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Common;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces;

namespace SmartWatch4G.Infrastructure.Services;

public sealed class GpsQueryService : IGpsQueryService
{
    private const string UnexpectedError = "An unexpected error occurred.";

    private readonly IDatabaseService    _db;
    private readonly IDeviceStatusCache  _statusCache;
    private readonly IwownService        _iwown;
    private readonly ILogger<GpsQueryService> _logger;

    public GpsQueryService(IDatabaseService db, IDeviceStatusCache statusCache,
        IwownService iwown, ILogger<GpsQueryService> logger)
    {
        _db          = db;
        _statusCache = statusCache;
        _iwown       = iwown;
        _logger      = logger;
    }

    public async Task<ServiceResult<GpsPagedResult>> GetByCompanyAsync(int companyId, GpsQueryParams q)
    {
        try
        {
            ValidateDateRange(q.From, q.To, out var err);
            if (err is not null) return ServiceResult<GpsPagedResult>.Fail(err, 400);

            // Refresh status from Iwown in real-time so online/offline counts are accurate
            await RefreshCompanyStatusAsync(companyId);

            var (skip, take) = Paging(q);
            var (items, totalCount) = await _db.GetGnssTracksByCompany(
                companyId, q.From, q.To, skip, take, q.SortDir,
                onlineOnly: false, offlineOnly: false);

            var onlineIds = _statusCache.GetAllDeviceIds().Where(_statusCache.IsOnline).ToList();
            var (online, offline) = await _db.GetDeviceStatusCountsByCompany(companyId, onlineIds);

            return ServiceResult<GpsPagedResult>.Ok(Build(items, totalCount, q, online, offline));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GpsQueryService.GetByCompanyAsync failed for company {Id}", companyId);
            return ServiceResult<GpsPagedResult>.Fail(UnexpectedError, 500);
        }
    }

    public async Task<ServiceResult<GpsPagedResult>> GetOnlineByCompanyAsync(int companyId, GpsQueryParams q)
    {
        try
        {
            ValidateDateRange(q.From, q.To, out var err);
            if (err is not null) return ServiceResult<GpsPagedResult>.Fail(err, 400);

            // Call Iwown for every device in the company in real-time, then use the fresh cache
            await RefreshCompanyStatusAsync(companyId);

            var (skip, take) = Paging(q);
            var (items, _) = await _db.GetGnssTracksByCompany(
                companyId, q.From, q.To, skip, take, q.SortDir,
                onlineOnly: true, offlineOnly: false);

            var onlineIds = new HashSet<string>(_statusCache.GetAllDeviceIds().Where(_statusCache.IsOnline),
                StringComparer.OrdinalIgnoreCase);
            var filtered = items.Where(x => onlineIds.Contains(x.DeviceId)).ToList();

            var (online, offline) = await _db.GetDeviceStatusCountsByCompany(companyId, onlineIds.ToList());
            return ServiceResult<GpsPagedResult>.Ok(
                Build(filtered, filtered.Count, q, online, offline));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GpsQueryService.GetOnlineByCompanyAsync failed for company {Id}", companyId);
            return ServiceResult<GpsPagedResult>.Fail(UnexpectedError, 500);
        }
    }

    public async Task<ServiceResult<GpsPagedResult>> GetOfflineByCompanyAsync(int companyId, GpsQueryParams q)
    {
        try
        {
            ValidateDateRange(q.From, q.To, out var err);
            if (err is not null) return ServiceResult<GpsPagedResult>.Fail(err, 400);

            // Call Iwown for every device in the company in real-time, then use the fresh cache
            await RefreshCompanyStatusAsync(companyId);

            var (skip, take) = Paging(q);
            var (items, _) = await _db.GetGnssTracksByCompany(
                companyId, q.From, q.To, skip, take, q.SortDir,
                onlineOnly: false, offlineOnly: true);

            var onlineIds = new HashSet<string>(_statusCache.GetAllDeviceIds().Where(_statusCache.IsOnline),
                StringComparer.OrdinalIgnoreCase);
            var filtered = items.Where(x => !onlineIds.Contains(x.DeviceId)).ToList();

            var (online, offline) = await _db.GetDeviceStatusCountsByCompany(companyId, onlineIds.ToList());
            return ServiceResult<GpsPagedResult>.Ok(
                Build(filtered, filtered.Count, q, online, offline));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GpsQueryService.GetOfflineByCompanyAsync failed for company {Id}", companyId);
            return ServiceResult<GpsPagedResult>.Fail(UnexpectedError, 500);
        }
    }

    private async Task RefreshCompanyStatusAsync(int companyId)
    {
        var profiles = await _db.GetUsersByCompanyId(companyId);
        if (profiles.Count == 0) return;

        await Task.WhenAll(profiles.Select(async p =>
        {
            var response = await _iwown.GetDeviceStatusAsync(p.DeviceId);
            _statusCache.SetStatus(p.DeviceId, DeviceStatusParser.IsOnline(response));
        }));
    }

    public async Task<ServiceResult<GpsPagedResult>> GetByDeviceAsync(string deviceId, GpsQueryParams q)
    {
        try
        {
            ValidateDateRange(q.From, q.To, out var err);
            if (err is not null) return ServiceResult<GpsPagedResult>.Fail(err, 400);

            var tracks = await _db.GetGnssTracks(deviceId, q.From, q.To);

            var sorted = string.Equals(q.SortDir, "asc", StringComparison.OrdinalIgnoreCase)
                ? tracks.OrderBy(t => t.CreatedAt).ToList()
                : tracks.OrderByDescending(t => t.CreatedAt).ToList();

            var total = sorted.Count;
            var (skip, take) = Paging(q);
            var page = sorted.Skip(skip).Take(take).ToList();

            var result = new GpsPagedResult
            {
                Items        = page.Select(t => MapTrack(deviceId, null, t)).ToList(),
                TotalCount   = total,
                Page         = q.Page,
                PageSize     = q.PageSize,
                OnlineCount  = _statusCache.IsOnline(deviceId) ? 1 : 0,
                OfflineCount = _statusCache.IsOnline(deviceId) ? 0 : 1
            };

            return ServiceResult<GpsPagedResult>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GpsQueryService.GetByDeviceAsync failed for {Device}", deviceId);
            return ServiceResult<GpsPagedResult>.Fail(UnexpectedError, 500);
        }
    }

    public async Task<ServiceResult<DeviceGpsStatusResponse>> GetDeviceGpsStatusAsync(string deviceId)
    {
        try
        {
            // All three run in parallel: real-time Iwown status + latest GPS + user profile
            var statusTask  = _iwown.GetDeviceStatusAsync(deviceId);
            var trackTask   = _db.GetLatestGnssTrack(deviceId);
            var profileTask = _db.GetUserProfile(deviceId);
            await Task.WhenAll(statusTask, trackTask, profileTask);

            var isOnline = DeviceStatusParser.IsOnline(statusTask.Result);
            // Keep the polling cache in sync so dashboard queries stay consistent
            _statusCache.SetStatus(deviceId, isOnline);

            var track   = trackTask.Result;
            var profile = profileTask.Result;

            return ServiceResult<DeviceGpsStatusResponse>.Ok(new DeviceGpsStatusResponse
            {
                DeviceId   = deviceId,
                UserName   = profile is null ? null : $"{profile.Name} {profile.Surname}",
                Status     = isOnline ? "online" : "offline",
                StatusCode = isOnline ? 1 : 0,
                GnssTime   = track?.GnssTime,
                Latitude   = track?.Latitude,
                Longitude  = track?.Longitude,
                LocType    = track?.LocType,
                RecordedAt = track?.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDeviceGpsStatusAsync failed for {Device}", deviceId);
            return ServiceResult<DeviceGpsStatusResponse>.Fail(UnexpectedError, 500);
        }
    }

    public async Task<ServiceResult<IReadOnlyList<DeviceMapResponse>>> GetMapDataAsync(int companyId, System.DateTime? date)
    {
        try
        {
            var day  = (date ?? System.DateTime.Today).Date;
            var from = day;
            var to   = day.AddDays(1).AddTicks(-1);

            var profiles = await _db.GetUsersByCompanyId(companyId);
            if (profiles.Count == 0)
                return ServiceResult<IReadOnlyList<DeviceMapResponse>>.Ok([]);

            // Fetch tracks, health, and online status for all devices in parallel
            var deviceData = await Task.WhenAll(profiles.Select(async p =>
            {
                var tracksTask = _db.GetGnssTracks(p.DeviceId, from, to);
                var healthTask = _db.GetLatestHealthSnapshot(p.DeviceId);
                var statusTask = _iwown.GetDeviceStatusAsync(p.DeviceId);
                await Task.WhenAll(tracksTask, healthTask, statusTask);

                var isOnline = DeviceStatusParser.IsOnline(statusTask.Result);
                _statusCache.SetStatus(p.DeviceId, isOnline);

                var tracks = tracksTask.Result
                    .OrderByDescending(t => t.CreatedAt)
                    .Select(t => new MapTrackPoint
                    {
                        GnssTime   = t.GnssTime,
                        Latitude   = t.Latitude,
                        Longitude  = t.Longitude,
                        LocType    = t.LocType,
                        RecordedAt = t.CreatedAt
                    })
                    .ToList();

                var h = healthTask.Result;
                var health = h is null ? null : new MapHealthSnapshot
                {
                    RecordTime = h.RecordTime,
                    Battery    = h.Battery,
                    HeartRate  = h.AvgHr,
                    MaxHr      = h.MaxHr,
                    MinHr      = h.MinHr,
                    SpO2       = h.AvgSpo2,
                    Sbp        = h.Sbp,
                    Dbp        = h.Dbp,
                    Fatigue    = h.Fatigue,
                    Steps      = h.Steps,
                    Distance   = h.Distance,
                    Calorie    = h.Calorie,
                    RecordedAt = h.CreatedAt
                };

                return new DeviceMapResponse
                {
                    DeviceId     = p.DeviceId,
                    UserName     = $"{p.Name} {p.Surname}".Trim(),
                    EmpNo        = p.EmpNo,
                    Status       = isOnline ? "online" : "offline",
                    StatusCode   = isOnline ? 1 : 0,
                    Date         = day,
                    Tracks       = tracks,
                    LatestHealth = health
                };
            }));

            return ServiceResult<IReadOnlyList<DeviceMapResponse>>.Ok(deviceData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GpsQueryService.GetMapDataAsync failed for company {Id}", companyId);
            return ServiceResult<IReadOnlyList<DeviceMapResponse>>.Fail(UnexpectedError, 500);
        }
    }

    public async Task<ServiceResult<DeviceMapResponse>> GetDeviceMapDataAsync(string deviceId, System.DateTime? date)
    {
        try
        {
            var day  = (date ?? System.DateTime.Today).Date;
            var from = day;
            var to   = day.AddDays(1).AddTicks(-1);

            var tracksTask  = _db.GetGnssTracks(deviceId, from, to);
            var healthTask  = _db.GetLatestHealthSnapshot(deviceId);
            var profileTask = _db.GetUserProfile(deviceId);
            await Task.WhenAll(tracksTask, healthTask, profileTask);

            var profile = profileTask.Result;
            if (profile is null)
                return ServiceResult<DeviceMapResponse>.Fail("Device not found.", 404);

            var isOnline = _statusCache.IsOnline(deviceId);

            var tracks = tracksTask.Result
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new MapTrackPoint
                {
                    GnssTime   = t.GnssTime,
                    Latitude   = t.Latitude,
                    Longitude  = t.Longitude,
                    LocType    = t.LocType,
                    RecordedAt = t.CreatedAt
                })
                .ToList();

            var h = healthTask.Result;
            var health = h is null ? null : new MapHealthSnapshot
            {
                RecordTime = h.RecordTime,
                Battery    = h.Battery,
                HeartRate  = h.AvgHr,
                MaxHr      = h.MaxHr,
                MinHr      = h.MinHr,
                SpO2       = h.AvgSpo2,
                Sbp        = h.Sbp,
                Dbp        = h.Dbp,
                Fatigue    = h.Fatigue,
                Steps      = h.Steps,
                Distance   = h.Distance,
                Calorie    = h.Calorie,
                RecordedAt = h.CreatedAt
            };

            return ServiceResult<DeviceMapResponse>.Ok(new DeviceMapResponse
            {
                DeviceId     = deviceId,
                UserName     = $"{profile.Name} {profile.Surname}".Trim(),
                EmpNo        = profile.EmpNo,
                Status       = isOnline ? "online" : "offline",
                StatusCode   = isOnline ? 1 : 0,
                Date         = day,
                Tracks       = tracks,
                LatestHealth = health
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GpsQueryService.GetDeviceMapDataAsync failed for {Device}", deviceId);
            return ServiceResult<DeviceMapResponse>.Fail(UnexpectedError, 500);
        }
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static (int skip, int take) Paging(GpsQueryParams q)
    {
        var page     = Math.Max(1, q.Page);
        var pageSize = Math.Clamp(q.PageSize, 1, 500);
        return ((page - 1) * pageSize, pageSize);
    }

    private static void ValidateDateRange(System.DateTime? from, System.DateTime? to, out string? error)
    {
        error = (from.HasValue && to.HasValue && from.Value > to.Value)
            ? "'from' must be earlier than or equal to 'to'."
            : null;
    }

    private static GpsPagedResult Build(
        IReadOnlyList<(string DeviceId, string? UserName, GnssTrack Track)> items,
        int total, GpsQueryParams q, int online, int offline) =>
        new()
        {
            Items        = items.Select(x => MapTrack(x.DeviceId, x.UserName, x.Track)).ToList(),
            TotalCount   = total,
            Page         = q.Page,
            PageSize     = q.PageSize,
            OnlineCount  = online,
            OfflineCount = offline
        };

    private static GpsTrackResponse MapTrack(string deviceId, string? userName, GnssTrack t) => new()
    {
        DeviceId   = deviceId,
        UserName   = userName,
        GnssTime   = t.GnssTime,
        Latitude   = t.Latitude,
        Longitude  = t.Longitude,
        LocType    = t.LocType,
        RecordedAt = t.CreatedAt
    };
}
