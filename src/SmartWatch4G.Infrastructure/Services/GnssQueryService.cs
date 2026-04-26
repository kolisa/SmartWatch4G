using Microsoft.Extensions.Logging;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Common;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces;

namespace SmartWatch4G.Infrastructure.Services;

public sealed class GnssQueryService : IGnssQueryService
{
    private readonly IDatabaseService _db;
    private readonly IDeviceStatusCache _statusCache;
    private readonly ILogger<GnssQueryService> _logger;

    public GnssQueryService(IDatabaseService db, IDeviceStatusCache statusCache, ILogger<GnssQueryService> logger)
    {
        _db          = db;
        _statusCache = statusCache;
        _logger      = logger;
    }

    public async Task<ServiceResult<IReadOnlyList<OnlineUserTrackResponse>>> GetOnlineUsersWithTrackingAsync()
    {
        try
        {
            var users = await _db.GetAllUserProfiles();
            if (users.Count == 0)
                return ServiceResult<IReadOnlyList<OnlineUserTrackResponse>>.Ok([]);

            var tasks = users
                .Where(u => _statusCache.IsOnline(u.DeviceId))
                .Select(async u =>
                {
                    var track = await _db.GetLatestGnssTrack(u.DeviceId);
                    if (track is null) return null;

                    return new OnlineUserTrackResponse
                    {
                        User         = MapUser(u),
                        LatestTrack  = MapTrack(track),
                        DeviceStatus = "online"
                    };
                });

            var results = await Task.WhenAll(tasks);
            IReadOnlyList<OnlineUserTrackResponse> online = results
                .OfType<OnlineUserTrackResponse>()
                .ToList();

            return ServiceResult<IReadOnlyList<OnlineUserTrackResponse>>.Ok(online);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetOnlineUsersWithTrackingAsync failed");
            return ServiceResult<IReadOnlyList<OnlineUserTrackResponse>>.Fail("An unexpected error occurred.", 500);
        }
    }

    public async Task<ServiceResult<UserTrackHistoryResponse>> GetTrackHistoryAsync(
        string deviceId, System.DateTime? from, System.DateTime? to)
    {
        try
        {
            var tracks       = await _db.GetGnssTracks(deviceId, from, to);
            var deviceStatus = _statusCache.GetStatus(deviceId);

            var response = new UserTrackHistoryResponse
            {
                DeviceId     = deviceId,
                DeviceStatus = deviceStatus,
                TrackHistory = tracks.Select(MapTrack).ToList()
            };

            return ServiceResult<UserTrackHistoryResponse>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTrackHistoryAsync failed for DeviceId {DeviceId}", deviceId);
            return ServiceResult<UserTrackHistoryResponse>.Fail("An unexpected error occurred.", 500);
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
