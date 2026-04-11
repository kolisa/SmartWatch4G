using Dapper;

using Microsoft.EntityFrameworkCore;

using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Persistence.Repositories;

internal sealed class HealthDataRepository : IHealthDataRepository
{
    private readonly AppDbContext _db;

    public HealthDataRepository(AppDbContext db) => _db = db;

    public Task AddAsync(HealthDataRecord record, CancellationToken cancellationToken = default)
    {
        _db.HealthDataRecords.Add(record);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<HealthDataRecord>> GetByDeviceAndDateAsync(
        string deviceId,
        string date,
        CancellationToken cancellationToken = default)
    {
        string from = $"{date} 00:00:00";
        string to = $"{date} 23:59:59";

        return await _db.HealthDataRecords
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId
                        && string.Compare(x.DataTime, from, StringComparison.Ordinal) >= 0
                        && string.Compare(x.DataTime, to, StringComparison.Ordinal) <= 0)
            .OrderBy(x => x.DataTime)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<HealthDataRecord>> GetByDeviceAndTimeRangeAsync(
        string deviceId,
        string fromTime,
        string toTime,
        CancellationToken cancellationToken = default)
    {
        return await _db.HealthDataRecords
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId
                        && string.Compare(x.DataTime, fromTime, StringComparison.Ordinal) >= 0
                        && string.Compare(x.DataTime, toTime, StringComparison.Ordinal) <= 0)
            .OrderBy(x => x.DataTime)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<HealthDataRecord?> GetLatestByDeviceAsync(
        string deviceId,
        CancellationToken cancellationToken = default)
        => _db.HealthDataRecords
              .AsNoTracking()
              .Where(x => x.DeviceId == deviceId)
              .OrderByDescending(x => x.DataTime)
              .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<HealthDataRecord>> GetLatestAllDevicesAsync(
        CancellationToken cancellationToken = default)
    {
        // ROW_NUMBER() window function: one pass over the table, no cross-join.
        // With 100 000 devices the old GroupBy+Join created a correlated sub-query
        // that caused a full table scan for every device.
        return await _db.HealthDataRecords
            .FromSqlRaw("""
                SELECT *
                FROM (
                    SELECT *,
                           ROW_NUMBER() OVER (PARTITION BY DeviceId ORDER BY DataTime DESC) AS _rn
                    FROM   HealthDataRecords
                ) t
                WHERE _rn = 1
                ORDER BY DeviceId
                """)
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<HealthDataRecord>> GetAllDevicesAndDateAsync(
        string date,
        CancellationToken cancellationToken = default)
    {
        string from = $"{date} 00:00:00";
        string to = $"{date} 23:59:59";

        return await _db.HealthDataRecords
            .AsNoTracking()
            .Where(x => string.Compare(x.DataTime, from, StringComparison.Ordinal) >= 0
                        && string.Compare(x.DataTime, to, StringComparison.Ordinal) <= 0)
            .OrderBy(x => x.DeviceId)
            .ThenBy(x => x.DataTime)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
