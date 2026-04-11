using SmartWatch4G.Domain.Entities;

namespace SmartWatch4G.Domain.Interfaces.Repositories;

public interface IEcgDataRepository
{
    Task AddAsync(EcgDataRecord record, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EcgDataRecord>> GetByDeviceAndDateAsync(
        string deviceId,
        string date,
        CancellationToken cancellationToken = default);
}
