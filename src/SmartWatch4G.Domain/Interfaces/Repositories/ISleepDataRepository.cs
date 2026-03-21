using SmartWatch4G.Domain.Entities;

namespace SmartWatch4G.Domain.Interfaces.Repositories;

public interface ISleepDataRepository
{
    Task AddAsync(SleepDataRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all sleep-slot records for a device on a given date (yyyy-MM-dd),
    /// ordered by seq ascending — ready to combine into the algo-service string.
    /// </summary>
    Task<IReadOnlyList<SleepDataRecord>> GetByDeviceAndDateAsync(
        string deviceId,
        string sleepDate,
        CancellationToken cancellationToken = default);
}
