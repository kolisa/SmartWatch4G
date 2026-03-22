using SmartWatch4G.Domain.Entities;

namespace SmartWatch4G.Domain.Interfaces.Repositories;

public interface IGnssTrackRepository
{
    Task AddRangeAsync(IEnumerable<GnssTrackRecord> records, CancellationToken cancellationToken = default);

    /// <summary>Returns track points for a device on a specific day (yyyy-MM-dd).</summary>
    Task<IReadOnlyList<GnssTrackRecord>> GetByDeviceAndDateAsync(string deviceId, string date, CancellationToken cancellationToken = default);

    /// <summary>Returns track points for a device within an explicit datetime range (yyyy-MM-dd HH:mm:ss).</summary>
    Task<IReadOnlyList<GnssTrackRecord>> GetByDeviceAndTimeRangeAsync(string deviceId, string fromTime, string toTime, CancellationToken cancellationToken = default);

    /// <summary>Returns the track points received in the last <paramref name="minutes"/> minutes for a device.</summary>
    Task<IReadOnlyList<GnssTrackRecord>> GetRecentByDeviceAsync(string deviceId, int minutes, CancellationToken cancellationToken = default);

    /// <summary>Returns the single most recent track point for a device, or null if none exists.</summary>
    Task<GnssTrackRecord?> GetLatestByDeviceAsync(string deviceId, CancellationToken cancellationToken = default);

    /// <summary>Returns the most recent track point for every device that has location data.</summary>
    Task<IReadOnlyList<GnssTrackRecord>> GetLatestAllDevicesAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns all track points for all devices on a specific day.</summary>
    Task<IReadOnlyList<GnssTrackRecord>> GetAllDevicesAndDateAsync(string date, CancellationToken cancellationToken = default);
}
