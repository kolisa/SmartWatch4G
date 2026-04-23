using Microsoft.Extensions.Logging;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Common;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces;
using SysDateTime = System.DateTime;

namespace SmartWatch4G.Infrastructure.Services;

public sealed class GpsQueryService : IGpsQueryService
{
    private const string UnexpectedError = "An unexpected error occurred.";

    private readonly IDatabaseService    _db;
    private readonly IDeviceStatusCache  _statusCache;
    private readonly ILogger<GpsQueryService> _logger;

    public GpsQueryService(IDatabaseService db, IDeviceStatusCache statusCache, ILogger<GpsQueryService> logger)
    {
        _db          = db;
        _statusCache = statusCache;
        _logger      = logger;
    }

    public Task<ServiceResult<GpsPagedResult>> GetByCompanyAsync(int companyId, GpsQueryParams q)
    {
        try
        {
            ValidateDateRange(q.From, q.To, out var err);
            if (err is not null) return Task.FromResult(ServiceResult<GpsPagedResult>.Fail(err, 400));

            var (skip, take) = Paging(q);
            var (items, totalCount) = _db.GetGnssTracksByCompany(
                companyId, q.From, q.To, skip, take, q.SortDir,
                onlineOnly: false, offlineOnly: false);

            var onlineIds = _statusCache.GetAllDeviceIds().Where(_statusCache.IsOnline).ToList();
            var (online, offline) = _db.GetDeviceStatusCountsByCompany(companyId, onlineIds);

            return Task.FromResult(ServiceResult<GpsPagedResult>.Ok(Build(items, totalCount, q, online, offline)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GpsQueryService.GetByCompanyAsync failed for company {Id}", companyId);
            return Task.FromResult(ServiceResult<GpsPagedResult>.Fail(UnexpectedError, 500));
        }
    }

    public Task<ServiceResult<GpsPagedResult>> GetOnlineByCompanyAsync(int companyId, GpsQueryParams q)
    {
        try
        {
            ValidateDateRange(q.From, q.To, out var err);
            if (err is not null) return Task.FromResult(ServiceResult<GpsPagedResult>.Fail(err, 400));

            var (skip, take) = Paging(q);
            var (items, _) = _db.GetGnssTracksByCompany(
                companyId, q.From, q.To, skip, take, q.SortDir,
                onlineOnly: true, offlineOnly: false);

            // filter to online devices using cache
            var onlineIds = new HashSet<string>(_statusCache.GetAllDeviceIds().Where(_statusCache.IsOnline),
                StringComparer.OrdinalIgnoreCase);
            var filtered = items.Where(x => onlineIds.Contains(x.DeviceId)).ToList();

            var (online, offline) = _db.GetDeviceStatusCountsByCompany(companyId, onlineIds.ToList());
            return Task.FromResult(ServiceResult<GpsPagedResult>.Ok(
                Build(filtered, filtered.Count, q, online, offline)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GpsQueryService.GetOnlineByCompanyAsync failed for company {Id}", companyId);
            return Task.FromResult(ServiceResult<GpsPagedResult>.Fail(UnexpectedError, 500));
        }
    }

    public Task<ServiceResult<GpsPagedResult>> GetOfflineByCompanyAsync(int companyId, GpsQueryParams q)
    {
        try
        {
            ValidateDateRange(q.From, q.To, out var err);
            if (err is not null) return Task.FromResult(ServiceResult<GpsPagedResult>.Fail(err, 400));

            var (skip, take) = Paging(q);
            var (items, _) = _db.GetGnssTracksByCompany(
                companyId, q.From, q.To, skip, take, q.SortDir,
                onlineOnly: false, offlineOnly: true);

            var onlineIds = new HashSet<string>(_statusCache.GetAllDeviceIds().Where(_statusCache.IsOnline),
                StringComparer.OrdinalIgnoreCase);
            var filtered = items.Where(x => !onlineIds.Contains(x.DeviceId)).ToList();

            var (online, offline) = _db.GetDeviceStatusCountsByCompany(companyId, onlineIds.ToList());
            return Task.FromResult(ServiceResult<GpsPagedResult>.Ok(
                Build(filtered, filtered.Count, q, online, offline)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GpsQueryService.GetOfflineByCompanyAsync failed for company {Id}", companyId);
            return Task.FromResult(ServiceResult<GpsPagedResult>.Fail(UnexpectedError, 500));
        }
    }

    public Task<ServiceResult<GpsPagedResult>> GetByDeviceAsync(string deviceId, GpsQueryParams q)
    {
        try
        {
            ValidateDateRange(q.From, q.To, out var err);
            if (err is not null) return Task.FromResult(ServiceResult<GpsPagedResult>.Fail(err, 400));

            var tracks = _db.GetGnssTracks(deviceId, q.From, q.To);

            // Sort in memory (single-device queries are typically small)
            var sorted = string.Equals(q.SortDir, "asc", StringComparison.OrdinalIgnoreCase)
                ? tracks.OrderBy(t => t.CreatedAt).ToList()
                : tracks.OrderByDescending(t => t.CreatedAt).ToList();

            var total = sorted.Count;
            var (skip, take) = Paging(q);
            var page = sorted.Skip(skip).Take(take).ToList();

            var mapped = page.Select(t => MapTrack(deviceId, null, t)).ToList();
            var result = new GpsPagedResult
            {
                Items        = mapped,
                TotalCount   = total,
                Page         = q.Page,
                PageSize     = q.PageSize,
                OnlineCount  = _statusCache.IsOnline(deviceId) ? 1 : 0,
                OfflineCount = _statusCache.IsOnline(deviceId) ? 0 : 1
            };

            return Task.FromResult(ServiceResult<GpsPagedResult>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GpsQueryService.GetByDeviceAsync failed for {Device}", deviceId);
            return Task.FromResult(ServiceResult<GpsPagedResult>.Fail(UnexpectedError, 500));
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
