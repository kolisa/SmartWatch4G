using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using SmartWatch4G.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SmartWatch4G.Infrastructure.Persistence.Repositories;

internal sealed class CallLogRepository : ICallLogRepository
{
    private readonly AppDbContext _db;

    public CallLogRepository(AppDbContext db) => _db = db;

    public async Task AddRangeAsync(IEnumerable<CallLogRecord> records, CancellationToken cancellationToken = default)
    {
        _db.CallLogRecords.AddRange(records);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<CallLogRecord>> GetByDeviceAndDateAsync(
        string deviceId,
        string date,
        CancellationToken cancellationToken = default)
    {
        System.DateTime day = DateTimeUtilities.TryParseDate(date)
            ?? throw new ArgumentException($"Invalid date: '{date}'", nameof(date));
        System.DateTime nextDay = day.AddDays(1);

        return await _db.CallLogRecords
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId
                        && x.ReceivedAt >= day
                        && x.ReceivedAt < nextDay)
            .OrderByDescending(x => x.ReceivedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<CallLogRecord>> GetByDeviceAndTimeRangeAsync(
        string deviceId,
        string fromTime,
        string toTime,
        CancellationToken cancellationToken = default)
    {
        System.DateTime from = DateTimeUtilities.TryParseDateTime(fromTime)
            ?? throw new ArgumentException($"Invalid datetime: '{fromTime}'", nameof(fromTime));
        System.DateTime to = DateTimeUtilities.TryParseDateTime(toTime)
            ?? throw new ArgumentException($"Invalid datetime: '{toTime}'", nameof(toTime));

        return await _db.CallLogRecords
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId
                        && x.ReceivedAt >= from
                        && x.ReceivedAt <= to)
            .OrderByDescending(x => x.ReceivedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<CallLogRecord>> GetAllDevicesAndDateAsync(
        string date,
        CancellationToken cancellationToken = default)
    {
        System.DateTime day = DateTimeUtilities.TryParseDate(date)
            ?? throw new ArgumentException($"Invalid date: '{date}'", nameof(date));
        System.DateTime nextDay = day.AddDays(1);

        return await _db.CallLogRecords
            .AsNoTracking()
            .Where(x => x.ReceivedAt >= day && x.ReceivedAt < nextDay)
            .OrderBy(x => x.DeviceId)
            .ThenByDescending(x => x.ReceivedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
