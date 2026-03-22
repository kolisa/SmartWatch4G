using SmartWatch4G.Domain.Common;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using SmartWatch4G.Domain.Interfaces.Services;

namespace SmartWatch4G.FunctionalTests.Stubs;

// ── Repository stubs — return empty data by default ──────────────────────────

internal sealed class StubDeviceInfoRepository : IDeviceInfoRepository
{
    public Task UpsertAsync(DeviceInfoRecord record, CancellationToken ct = default) => Task.CompletedTask;
    public Task<DeviceInfoRecord?> FindByDeviceIdAsync(string deviceId, CancellationToken ct = default) => Task.FromResult<DeviceInfoRecord?>(null);
    public Task<IReadOnlyList<DeviceInfoRecord>> GetAllAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<DeviceInfoRecord>>([]);
}

internal sealed class StubDeviceStatusRepository : IDeviceStatusRepository
{
    public Task AddAsync(DeviceStatusRecord record, CancellationToken ct = default) => Task.CompletedTask;
    public Task<IReadOnlyList<DeviceStatusRecord>> GetByDeviceAndDateAsync(string deviceId, string date, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<DeviceStatusRecord>>([]);
    public Task<DeviceStatusRecord?> GetLatestByDeviceAsync(string deviceId, CancellationToken ct = default) => Task.FromResult<DeviceStatusRecord?>(null);
    public Task<IReadOnlyList<DeviceStatusRecord>> GetLatestAllDevicesAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<DeviceStatusRecord>>([]);
}

internal sealed class StubAlarmRepository : IAlarmRepository
{
    public Task AddRangeAsync(IEnumerable<AlarmEventRecord> records, CancellationToken ct = default) => Task.CompletedTask;
    public Task<IReadOnlyList<AlarmEventRecord>> GetByDeviceAndDateAsync(string deviceId, string date, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<AlarmEventRecord>>([]);
    public Task<IReadOnlyList<AlarmEventRecord>> GetByDeviceAndTimeRangeAsync(string deviceId, string fromTime, string toTime, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<AlarmEventRecord>>([]);
    public Task<AlarmEventRecord?> GetLatestByDeviceAsync(string deviceId, CancellationToken ct = default) => Task.FromResult<AlarmEventRecord?>(null);
    public Task<IReadOnlyList<AlarmEventRecord>> GetLatestAllDevicesAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<AlarmEventRecord>>([]);
    public Task<IReadOnlyList<AlarmEventRecord>> GetAllDevicesAndDateAsync(string date, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<AlarmEventRecord>>([]);
}

internal sealed class StubCallLogRepository : ICallLogRepository
{
    public Task AddRangeAsync(IEnumerable<CallLogRecord> records, CancellationToken ct = default) => Task.CompletedTask;
    public Task<IReadOnlyList<CallLogRecord>> GetByDeviceAndDateAsync(string deviceId, string date, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<CallLogRecord>>([]);
    public Task<IReadOnlyList<CallLogRecord>> GetByDeviceAndTimeRangeAsync(string deviceId, string fromTime, string toTime, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<CallLogRecord>>([]);
    public Task<IReadOnlyList<CallLogRecord>> GetAllDevicesAndDateAsync(string date, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<CallLogRecord>>([]);
}

internal sealed class StubHealthDataRepository : IHealthDataRepository
{
    public Task AddAsync(HealthDataRecord record, CancellationToken ct = default) => Task.CompletedTask;
    public Task AddEcgAsync(EcgDataRecord record, CancellationToken ct = default) => Task.CompletedTask;
    public Task<IReadOnlyList<HealthDataRecord>> GetByDeviceAndDateAsync(string deviceId, string date, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<HealthDataRecord>>([]);
    public Task<IReadOnlyList<EcgDataRecord>> GetEcgByDeviceAndDateAsync(string deviceId, string date, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<EcgDataRecord>>([]);
    public Task<IReadOnlyList<HealthDataRecord>> GetByDeviceAndTimeRangeAsync(string deviceId, string fromTime, string toTime, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<HealthDataRecord>>([]);
    public Task<HealthDataRecord?> GetLatestByDeviceAsync(string deviceId, CancellationToken ct = default) => Task.FromResult<HealthDataRecord?>(null);
    public Task<IReadOnlyList<HealthDataRecord>> GetLatestAllDevicesAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<HealthDataRecord>>([]);
    public Task<IReadOnlyList<HealthDataRecord>> GetAllDevicesAndDateAsync(string date, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<HealthDataRecord>>([]);
}

internal sealed class StubGnssTrackRepository : IGnssTrackRepository
{
    public Task AddRangeAsync(IEnumerable<GnssTrackRecord> records, CancellationToken ct = default) => Task.CompletedTask;
    public Task<IReadOnlyList<GnssTrackRecord>> GetByDeviceAndDateAsync(string deviceId, string date, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<GnssTrackRecord>>([]);
    public Task<IReadOnlyList<GnssTrackRecord>> GetByDeviceAndTimeRangeAsync(string deviceId, string fromTime, string toTime, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<GnssTrackRecord>>([]);
    public Task<IReadOnlyList<GnssTrackRecord>> GetRecentByDeviceAsync(string deviceId, int minutes, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<GnssTrackRecord>>([]);
    public Task<GnssTrackRecord?> GetLatestByDeviceAsync(string deviceId, CancellationToken ct = default) => Task.FromResult<GnssTrackRecord?>(null);
    public Task<IReadOnlyList<GnssTrackRecord>> GetLatestAllDevicesAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<GnssTrackRecord>>([]);
    public Task<IReadOnlyList<GnssTrackRecord>> GetAllDevicesAndDateAsync(string date, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<GnssTrackRecord>>([]);
}

internal sealed class StubSleepDataRepository : ISleepDataRepository
{
    public Task AddAsync(SleepDataRecord record, CancellationToken ct = default) => Task.CompletedTask;
    public Task<IReadOnlyList<SleepDataRecord>> GetByDeviceAndDateAsync(string deviceId, string sleepDate, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<SleepDataRecord>>([]);
}

internal sealed class StubRriDataRepository : IRriDataRepository
{
    public Task AddAsync(RriDataRecord record, CancellationToken ct = default) => Task.CompletedTask;
    public Task<IReadOnlyList<RriDataRecord>> GetByDeviceAndDateAsync(string deviceId, string date, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<RriDataRecord>>([]);
}

internal sealed class StubSpo2DataRepository : ISpo2DataRepository
{
    public Task AddRangeAsync(IEnumerable<Spo2DataRecord> records, CancellationToken ct = default) => Task.CompletedTask;
    public Task<IReadOnlyList<Spo2DataRecord>> GetByDeviceAndDateRangeAsync(string deviceId, string fromTime, string toTime, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<Spo2DataRecord>>([]);
    public Task<Spo2DataRecord?> GetLatestByDeviceAsync(string deviceId, CancellationToken ct = default) => Task.FromResult<Spo2DataRecord?>(null);
    public Task<IReadOnlyList<Spo2DataRecord>> GetLatestAllDevicesAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<Spo2DataRecord>>([]);
}

internal sealed class StubAccDataRepository : IAccDataRepository
{
    public Task AddAsync(AccDataRecord record, CancellationToken ct = default) => Task.CompletedTask;
    public Task<IReadOnlyList<AccDataRecord>> GetByDeviceAndDateRangeAsync(string deviceId, string fromTime, string toTime, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<AccDataRecord>>([]);
}

internal sealed class StubSleepQueryService : ISleepQueryService
{
    public Task<ServiceResult<SleepResult?>> GetSleepResultAsync(string deviceId, string sleepDate, CancellationToken ct = default) =>
        Task.FromResult(ServiceResult<SleepResult?>.Ok(null));

    public Task<ServiceResult<IReadOnlyList<SleepResult>>> GetSleepResultsByDateRangeAsync(string deviceId, string fromDate, string toDate, CancellationToken ct = default) =>
        Task.FromResult(ServiceResult<IReadOnlyList<SleepResult>>.Ok(Array.Empty<SleepResult>()));
}
