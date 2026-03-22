using SmartWatch4G.Domain.Entities;

namespace SmartWatch4G.Domain.Interfaces.Repositories;

public interface IAlarmRepository
{
    Task AddRangeAsync(IEnumerable<AlarmEventRecord> records, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AlarmEventRecord>> GetByDeviceAndDateAsync(string deviceId, string date, CancellationToken cancellationToken = default);

    /// <summary>Returns alarm events for a device within an explicit datetime range (yyyy-MM-dd HH:mm:ss).</summary>
    Task<IReadOnlyList<AlarmEventRecord>> GetByDeviceAndTimeRangeAsync(string deviceId, string fromTime, string toTime, CancellationToken cancellationToken = default);

    /// <summary>Returns the single most recent alarm event for a device, or null if none exists.</summary>
    Task<AlarmEventRecord?> GetLatestByDeviceAsync(string deviceId, CancellationToken cancellationToken = default);

    /// <summary>Returns the most recent alarm event for every device that has alarm data.</summary>
    Task<IReadOnlyList<AlarmEventRecord>> GetLatestAllDevicesAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns alarm events for all devices on a specific day.</summary>
    Task<IReadOnlyList<AlarmEventRecord>> GetAllDevicesAndDateAsync(string date, CancellationToken cancellationToken = default);
}
