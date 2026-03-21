using SmartWatch4G.Domain.Entities;

namespace SmartWatch4G.Domain.Interfaces.Repositories;

public interface IRriDataRepository
{
    Task AddAsync(RriDataRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all RRI records for a device on a given date (yyyy-MM-dd),
    /// ordered by DataTime ascending.
    /// </summary>
    Task<IReadOnlyList<RriDataRecord>> GetByDeviceAndDateAsync(
        string deviceId,
        string date,
        CancellationToken cancellationToken = default);
}
