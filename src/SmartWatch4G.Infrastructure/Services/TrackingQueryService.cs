using Microsoft.Extensions.Logging;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Common;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces;

namespace SmartWatch4G.Infrastructure.Services;

public sealed class TrackingQueryService : ITrackingQueryService
{
    private readonly IDatabaseService _db;
    private readonly IDeviceStatusCache _statusCache;
    private readonly ILogger<TrackingQueryService> _logger;

    public TrackingQueryService(IDatabaseService db, IDeviceStatusCache statusCache, ILogger<TrackingQueryService> logger)
    {
        _db          = db;
        _statusCache = statusCache;
        _logger      = logger;
    }

    public Task<ServiceResult<IReadOnlyList<OnlineUserTrackResponse>>> GetOnlineUsersWithTrackingAsync()
    {
        try
        {
            var users = _db.GetAllUserProfiles();
            if (users.Count == 0)
                return Task.FromResult(ServiceResult<IReadOnlyList<OnlineUserTrackResponse>>.Ok([]));

            // Read from the in-memory cache (updated every 30 s by DeviceStatusPollingJob)
            IReadOnlyList<OnlineUserTrackResponse> online = users
                .Where(u => _statusCache.IsOnline(u.DeviceId))
                .Select(u =>
                {
                    var track = _db.GetLatestGnssTrack(u.DeviceId);
                    if (track is null) return null;

                    return new OnlineUserTrackResponse
                    {
                        User         = MapUser(u),
                        LatestTrack  = MapTrack(track),
                        DeviceStatus = "online"
                    };
                })
                .OfType<OnlineUserTrackResponse>()
                .ToList();

            return Task.FromResult(ServiceResult<IReadOnlyList<OnlineUserTrackResponse>>.Ok(online));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetOnlineUsersWithTrackingAsync failed");
            return Task.FromResult(ServiceResult<IReadOnlyList<OnlineUserTrackResponse>>.Fail("An unexpected error occurred.", 500));
        }
    }

    public Task<ServiceResult<UserTrackHistoryResponse>> GetTrackHistoryAsync(
        string deviceId, System.DateTime? from, System.DateTime? to)
    {
        try
        {
            var tracks       = _db.GetGnssTracks(deviceId, from, to);
            var deviceStatus = _statusCache.GetStatus(deviceId);

            var response = new UserTrackHistoryResponse
            {
                DeviceId     = deviceId,
                DeviceStatus = deviceStatus,
                TrackHistory = tracks.Select(MapTrack).ToList()
            };

            return Task.FromResult(ServiceResult<UserTrackHistoryResponse>.Ok(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTrackHistoryAsync failed for DeviceId {DeviceId}", deviceId);
            return Task.FromResult(ServiceResult<UserTrackHistoryResponse>.Fail("An unexpected error occurred.", 500));
        }
    }

    private static UserResponse MapUser(UserProfile p) => new()
    {
        DeviceId  = p.DeviceId,
        Name      = p.Name,
        Surname   = p.Surname,
        Email     = p.Email,
        Cell      = p.Cell,
        EmpNo     = p.EmpNo,
        Address   = p.Address,
        UpdatedAt = p.UpdatedAt
    };

    private static TrackPointResponse MapTrack(GnssTrack t) => new()
    {
        GnssTime  = t.GnssTime,
        Latitude  = t.Latitude,
        Longitude = t.Longitude,
        LocType   = t.LocType,
        CreatedAt = t.CreatedAt
    };
}

