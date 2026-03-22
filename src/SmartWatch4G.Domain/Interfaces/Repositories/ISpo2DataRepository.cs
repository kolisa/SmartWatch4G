using SmartWatch4G.Domain.Entities;

namespace SmartWatch4G.Domain.Interfaces.Repositories;

public interface ISpo2DataRepository
{
    Task AddRangeAsync(IEnumerable<Spo2DataRecord> records, CancellationToken cancellationToken = default);

    /// <summary>Returns all SPO2 records for a device within a time window (inclusive).</summary>
    Task<IReadOnlyList<Spo2DataRecord>> GetByDeviceAndDateRangeAsync(
        string deviceId,
        string fromTime,
        string toTime,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the single most recent SpO2 reading for a device, or null if none exists.</summary>
    Task<Spo2DataRecord?> GetLatestByDeviceAsync(string deviceId, CancellationToken cancellationToken = default);

    /// <summary>Returns the most recent SpO2 reading for every device that has SpO2 data.</summary>
    Task<IReadOnlyList<Spo2DataRecord>> GetLatestAllDevicesAsync(CancellationToken cancellationToken = default);
}
