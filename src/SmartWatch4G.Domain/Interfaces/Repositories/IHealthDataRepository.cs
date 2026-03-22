using SmartWatch4G.Domain.Entities;

namespace SmartWatch4G.Domain.Interfaces.Repositories;

public interface IHealthDataRepository
{
    Task AddAsync(HealthDataRecord record, CancellationToken cancellationToken = default);
    Task AddEcgAsync(EcgDataRecord record, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HealthDataRecord>> GetByDeviceAndDateAsync(string deviceId, string date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EcgDataRecord>> GetEcgByDeviceAndDateAsync(string deviceId, string date, CancellationToken cancellationToken = default);

    /// <summary>Returns health snapshots within an explicit datetime range (yyyy-MM-dd HH:mm:ss).</summary>
    Task<IReadOnlyList<HealthDataRecord>> GetByDeviceAndTimeRangeAsync(string deviceId, string fromTime, string toTime, CancellationToken cancellationToken = default);

    /// <summary>Returns the single most recent health snapshot for a device, or null if none exists.</summary>
    Task<HealthDataRecord?> GetLatestByDeviceAsync(string deviceId, CancellationToken cancellationToken = default);

    /// <summary>Returns the most recent health snapshot for every device that has health data.</summary>
    Task<IReadOnlyList<HealthDataRecord>> GetLatestAllDevicesAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns all health records for all devices on a specific day.</summary>
    Task<IReadOnlyList<HealthDataRecord>> GetAllDevicesAndDateAsync(string date, CancellationToken cancellationToken = default);
}
