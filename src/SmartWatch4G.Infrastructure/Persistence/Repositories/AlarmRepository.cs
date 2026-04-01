using Microsoft.EntityFrameworkCore;

using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Persistence.Repositories;

internal sealed class AlarmRepository : IAlarmRepository
{
    private readonly AppDbContext _db;

    public AlarmRepository(AppDbContext db) => _db = db;

    public async Task AddRangeAsync(IEnumerable<AlarmEventRecord> records, CancellationToken cancellationToken = default)
    {
        _db.AlarmEventRecords.AddRange(records);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<AlarmEventRecord>> GetByDeviceAndDateAsync(
        string deviceId,
        string date,
        CancellationToken cancellationToken = default)
    {
        System.DateTime day = DateTimeUtilities.TryParseDate(date)
            ?? throw new ArgumentException($"Invalid date: '{date}'", nameof(date));
        System.DateTime nextDay = day.AddDays(1);

        return await _db.AlarmEventRecords
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId
                        && x.ReceivedAt >= day
                        && x.ReceivedAt < nextDay)
            .OrderByDescending(x => x.ReceivedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<AlarmEventRecord>> GetByDeviceAndTimeRangeAsync(
        string deviceId,
        string fromTime,
        string toTime,
        CancellationToken cancellationToken = default)
    {
        System.DateTime from = DateTimeUtilities.TryParseDateTime(fromTime)
            ?? throw new ArgumentException($"Invalid datetime: '{fromTime}'", nameof(fromTime));
        System.DateTime to = DateTimeUtilities.TryParseDateTime(toTime)
            ?? throw new ArgumentException($"Invalid datetime: '{toTime}'", nameof(toTime));

        return await _db.AlarmEventRecords
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId
                        && x.ReceivedAt >= from
                        && x.ReceivedAt <= to)
            .OrderByDescending(x => x.ReceivedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<AlarmEventRecord?> GetLatestByDeviceAsync(
        string deviceId,
        CancellationToken cancellationToken = default)
        => _db.AlarmEventRecords
              .AsNoTracking()
              .Where(x => x.DeviceId == deviceId)
              .OrderByDescending(x => x.ReceivedAt)
              .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<AlarmEventRecord>> GetLatestAllDevicesAsync(
        CancellationToken cancellationToken = default)
    {
        var latestPerDevice = _db.AlarmEventRecords
            .GroupBy(r => r.DeviceId)
            .Select(g => new { DeviceId = g.Key, MaxReceivedAt = g.Max(r => r.ReceivedAt) });

        return await _db.AlarmEventRecords
            .AsNoTracking()
            .Join(latestPerDevice,
                  r => new { r.DeviceId, r.ReceivedAt },
                  l => new { l.DeviceId, ReceivedAt = l.MaxReceivedAt },
                  (r, _) => r)
            .OrderBy(r => r.DeviceId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<AlarmEventRecord>> GetAllDevicesAndDateAsync(
        string date,
        CancellationToken cancellationToken = default)
    {
        System.DateTime day = DateTimeUtilities.TryParseDate(date)
            ?? throw new ArgumentException($"Invalid date: '{date}'", nameof(date));
        System.DateTime nextDay = day.AddDays(1);

        return await _db.AlarmEventRecords
            .AsNoTracking()
            .Where(x => x.ReceivedAt >= day && x.ReceivedAt < nextDay)
            .OrderBy(x => x.DeviceId)
            .ThenByDescending(x => x.ReceivedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
