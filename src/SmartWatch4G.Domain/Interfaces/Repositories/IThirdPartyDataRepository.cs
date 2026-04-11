using SmartWatch4G.Domain.Entities;

namespace SmartWatch4G.Domain.Interfaces.Repositories;

public interface IThirdPartyDataRepository
{
    Task AddAsync(ThirdPartyDataRecord record, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ThirdPartyDataRecord>> GetByDeviceAndDateAsync(
        string deviceId,
        string date,
        CancellationToken cancellationToken = default);
}
