using SmartWatch4G.Domain.Entities;

namespace SmartWatch4G.Domain.Interfaces.Repositories;

public interface IPpgDataRepository
{
    Task AddAsync(PpgDataRecord record, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PpgDataRecord>> GetByDeviceAndDateAsync(
        string deviceId,
        string date,
        CancellationToken cancellationToken = default);
}
