using SmartWatch4G.Domain.Entities;

namespace SmartWatch4G.Domain.Interfaces.Repositories;

public interface IDeviceStatusRepository
{
    Task AddAsync(DeviceStatusRecord record, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeviceStatusRecord>> GetByDeviceAndDateAsync(string deviceId, string date, CancellationToken cancellationToken = default);

    /// <summary>Returns the single most recent status event for a device, or null if none exists.</summary>
    Task<DeviceStatusRecord?> GetLatestByDeviceAsync(string deviceId, CancellationToken cancellationToken = default);

    /// <summary>Returns the most recent status event for every device.</summary>
    Task<IReadOnlyList<DeviceStatusRecord>> GetLatestAllDevicesAsync(CancellationToken cancellationToken = default);
}
