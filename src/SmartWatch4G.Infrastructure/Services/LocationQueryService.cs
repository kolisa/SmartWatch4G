using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Services;

/// <summary>
/// Infrastructure implementation of <see cref="ILocationQueryService"/>.
/// </summary>
public sealed class LocationQueryService : ILocationQueryService
{
    private readonly IGnssTrackRepository _gnssRepo;

    public LocationQueryService(IGnssTrackRepository gnssRepo)
        => _gnssRepo = gnssRepo;

    public async Task<IReadOnlyList<LocationPointDto>> GetByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);
        var records = await _gnssRepo.GetByDeviceAndDateAsync(deviceId, date, ct)
            .ConfigureAwait(false);
        return Map(records, tzInfo);
    }

    public async Task<IReadOnlyList<LocationPointDto>> GetByRangeAsync(
        string deviceId, string from, string to, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);
        var records = await _gnssRepo.GetByDeviceAndTimeRangeAsync(deviceId, from, to, ct)
            .ConfigureAwait(false);
        return Map(records, tzInfo);
    }

    public async Task<IReadOnlyList<LocationPointDto>> GetRecentAsync(
        string deviceId, int minutes, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);
        var records = await _gnssRepo.GetRecentByDeviceAsync(deviceId, minutes, ct)
            .ConfigureAwait(false);
        return Map(records, tzInfo);
    }

    public async Task<LocationPointDto?> GetLatestAsync(
        string deviceId, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);
        var r = await _gnssRepo.GetLatestByDeviceAsync(deviceId, ct).ConfigureAwait(false);
        return r is null ? null : MapOne(r, tzInfo);
    }

    public async Task<IReadOnlyList<LocationPointDto>> GetLatestAllDevicesAsync(
        string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);
        var records = await _gnssRepo.GetLatestAllDevicesAsync(ct).ConfigureAwait(false);
        return Map(records, tzInfo);
    }

    public async Task<IReadOnlyList<LocationPointDto>> GetAllDevicesAndDateAsync(
        string date, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);
        var records = await _gnssRepo.GetAllDevicesAndDateAsync(date, ct).ConfigureAwait(false);
        return Map(records, tzInfo);
    }

    private static IReadOnlyList<LocationPointDto> Map(
        IEnumerable<Domain.Entities.GnssTrackRecord> records, TimeZoneInfo? tz)
        => records.Select(r => MapOne(r, tz)).ToList();

    private static LocationPointDto MapOne(Domain.Entities.GnssTrackRecord r, TimeZoneInfo? tz) => new()
    {
        DeviceId = r.DeviceId ?? string.Empty,
        TrackTime = DateTimeUtilities.LocalizeTimestamp(r.TrackTime, tz),
        Longitude = r.Longitude,
        Latitude = r.Latitude,
        GpsType = r.GpsType,
        BatteryLevel = r.BatteryLevel,
        Rssi = r.Rssi,
        Steps = r.Steps,
        DistanceMetres = r.DistanceMetres,
        CaloriesKcal = r.CaloriesKcal
    };
}
