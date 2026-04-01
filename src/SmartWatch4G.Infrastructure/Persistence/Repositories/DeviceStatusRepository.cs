using Microsoft.EntityFrameworkCore;

using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Persistence.Repositories;

internal sealed class DeviceStatusRepository : IDeviceStatusRepository
{
    private readonly AppDbContext _db;

    public DeviceStatusRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(DeviceStatusRecord record, CancellationToken cancellationToken = default)
    {
        _db.DeviceStatusRecords.Add(record);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<DeviceStatusRecord>> GetByDeviceAndDateAsync(
        string deviceId,
        string date,
        CancellationToken cancellationToken = default)
    {
        System.DateTime day = DateTimeUtilities.TryParseDate(date)
            ?? throw new ArgumentException($"Invalid date: '{date}'", nameof(date));
        System.DateTime nextDay = day.AddDays(1);

        return await _db.DeviceStatusRecords
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId
                        && x.ReceivedAt >= day
                        && x.ReceivedAt < nextDay)
            .OrderByDescending(x => x.ReceivedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<DeviceStatusRecord?> GetLatestByDeviceAsync(
        string deviceId,
        CancellationToken cancellationToken = default)
        => _db.DeviceStatusRecords
              .AsNoTracking()
              .Where(x => x.DeviceId == deviceId)
              .OrderByDescending(x => x.ReceivedAt)
              .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<DeviceStatusRecord>> GetLatestAllDevicesAsync(
        CancellationToken cancellationToken = default)
    {
        // Subquery: max ReceivedAt per device, then join to get the full record
        var latestPerDevice = _db.DeviceStatusRecords
            .GroupBy(r => r.DeviceId)
            .Select(g => new { DeviceId = g.Key, MaxReceivedAt = g.Max(r => r.ReceivedAt) });

        return await _db.DeviceStatusRecords
            .AsNoTracking()
            .Join(latestPerDevice,
                  r => new { r.DeviceId, r.ReceivedAt },
                  l => new { l.DeviceId, ReceivedAt = l.MaxReceivedAt },
                  (r, _) => r)
            .OrderBy(r => r.DeviceId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
