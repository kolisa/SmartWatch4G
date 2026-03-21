using SmartWatch4G.Domain.Entities;

namespace SmartWatch4G.Domain.Interfaces.Repositories;

public interface IDeviceInfoRepository
{
    Task UpsertAsync(DeviceInfoRecord record, CancellationToken cancellationToken = default);
    Task<DeviceInfoRecord?> FindByDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default);
}
