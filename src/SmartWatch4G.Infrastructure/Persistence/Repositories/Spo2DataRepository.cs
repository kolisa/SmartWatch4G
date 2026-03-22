using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using SmartWatch4G.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SmartWatch4G.Infrastructure.Persistence.Repositories;

internal sealed class Spo2DataRepository : ISpo2DataRepository
{
    private readonly AppDbContext _db;

    public Spo2DataRepository(AppDbContext db) => _db = db;

    public async Task AddRangeAsync(IEnumerable<Spo2DataRecord> records, CancellationToken cancellationToken = default)
    {
        _db.Spo2DataRecords.AddRange(records);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Spo2DataRecord>> GetByDeviceAndDateRangeAsync(
        string deviceId,
        string fromTime,
        string toTime,
        CancellationToken cancellationToken = default)
    {
        return await _db.Spo2DataRecords
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId
                        && string.Compare(x.DataTime, fromTime, StringComparison.Ordinal) >= 0
                        && string.Compare(x.DataTime, toTime, StringComparison.Ordinal) <= 0)
            .OrderBy(x => x.DataTime)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<Spo2DataRecord?> GetLatestByDeviceAsync(
        string deviceId,
        CancellationToken cancellationToken = default)
        => _db.Spo2DataRecords
              .AsNoTracking()
              .Where(x => x.DeviceId == deviceId)
              .OrderByDescending(x => x.DataTime)
              .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<Spo2DataRecord>> GetLatestAllDevicesAsync(
        CancellationToken cancellationToken = default)
    {
        var latestPerDevice = _db.Spo2DataRecords
            .GroupBy(r => r.DeviceId)
            .Select(g => new { DeviceId = g.Key, MaxDataTime = g.Max(r => r.DataTime)! });

        return await _db.Spo2DataRecords
            .AsNoTracking()
            .Join(latestPerDevice,
                  r => new { r.DeviceId, r.DataTime },
                  l => new { l.DeviceId, DataTime = l.MaxDataTime },
                  (r, _) => r)
            .OrderBy(r => r.DeviceId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
