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
}
