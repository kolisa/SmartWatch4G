using SmartWatch4G.Domain.Entities;

namespace SmartWatch4G.Domain.Interfaces.Repositories;

public interface IDeviceStatusRepository
{
    Task AddAsync(DeviceStatusRecord record, CancellationToken cancellationToken = default);
}
