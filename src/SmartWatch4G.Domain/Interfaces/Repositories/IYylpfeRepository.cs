using SmartWatch4G.Domain.Entities;

namespace SmartWatch4G.Domain.Interfaces.Repositories;

public interface IYylpfeRepository
{
    Task AddRangeAsync(IEnumerable<YylpfeRecord> records, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<YylpfeRecord>> GetByDeviceAndDateAsync(
        string deviceId,
        string date,
        CancellationToken cancellationToken = default);
}
