using SmartWatch4G.Domain.Entities;

namespace SmartWatch4G.Domain.Interfaces.Repositories;

public interface IMultiLeadsEcgRepository
{
    Task AddAsync(MultiLeadsEcgRecord record, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MultiLeadsEcgRecord>> GetByDeviceAndDateAsync(
        string deviceId,
        string date,
        CancellationToken cancellationToken = default);
}
