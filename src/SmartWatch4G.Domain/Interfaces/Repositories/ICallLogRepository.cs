using SmartWatch4G.Domain.Entities;

namespace SmartWatch4G.Domain.Interfaces.Repositories;

public interface ICallLogRepository
{
    Task AddRangeAsync(IEnumerable<CallLogRecord> records, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CallLogRecord>> GetByDeviceAndDateAsync(string deviceId, string date, CancellationToken cancellationToken = default);

    /// <summary>Returns call-log entries for a device within an explicit datetime range (yyyy-MM-dd HH:mm:ss).</summary>
    Task<IReadOnlyList<CallLogRecord>> GetByDeviceAndTimeRangeAsync(string deviceId, string fromTime, string toTime, CancellationToken cancellationToken = default);

    /// <summary>Returns call-log entries for all devices on a specific day.</summary>
    Task<IReadOnlyList<CallLogRecord>> GetAllDevicesAndDateAsync(string date, CancellationToken cancellationToken = default);
}
