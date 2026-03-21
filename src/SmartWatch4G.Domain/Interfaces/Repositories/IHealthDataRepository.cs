using SmartWatch4G.Domain.Entities;

namespace SmartWatch4G.Domain.Interfaces.Repositories;

public interface IHealthDataRepository
{
    Task AddAsync(HealthDataRecord record, CancellationToken cancellationToken = default);
    Task AddEcgAsync(EcgDataRecord record, CancellationToken cancellationToken = default);
}
