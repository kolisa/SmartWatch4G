using SmartWatch4G.Domain.Entities;

namespace SmartWatch4G.Domain.Interfaces.Repositories;

public interface IAccDataRepository
{
    Task AddAsync(AccDataRecord record, CancellationToken cancellationToken = default);

    /// <summary>Returns ACC records for a device within a time window (inclusive).</summary>
    Task<IReadOnlyList<AccDataRecord>> GetByDeviceAndDateRangeAsync(
        string deviceId,
        string fromTime,
        string toTime,
        CancellationToken cancellationToken = default);
}
