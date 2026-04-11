using System.Globalization;

using Dapper;

using Microsoft.EntityFrameworkCore;

using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Persistence.Repositories;

internal sealed class GnssTrackRepository : IGnssTrackRepository
{
    private readonly AppDbContext _db;

    public GnssTrackRepository(AppDbContext db) => _db = db;

    public Task AddRangeAsync(IEnumerable<GnssTrackRecord> records, CancellationToken cancellationToken = default)
    {
        _db.GnssTrackRecords.AddRange(records);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<GnssTrackRecord>> GetByDeviceAndDateAsync(
        string deviceId,
        string date,
        CancellationToken cancellationToken = default)
    {
        string from = $"{date} 00:00:00";
        string to = $"{date} 23:59:59";

        return await _db.GnssTrackRecords
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId
                        && string.Compare(x.TrackTime, from, StringComparison.Ordinal) >= 0
                        && string.Compare(x.TrackTime, to, StringComparison.Ordinal) <= 0)
            .OrderBy(x => x.TrackTime)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<GnssTrackRecord>> GetByDeviceAndTimeRangeAsync(
        string deviceId,
        string fromTime,
        string toTime,
        CancellationToken cancellationToken = default)
    {
        return await _db.GnssTrackRecords
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId
                        && string.Compare(x.TrackTime, fromTime, StringComparison.Ordinal) >= 0
                        && string.Compare(x.TrackTime, toTime, StringComparison.Ordinal) <= 0)
            .OrderBy(x => x.TrackTime)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<GnssTrackRecord>> GetRecentByDeviceAsync(
        string deviceId,
        int minutes,
        CancellationToken cancellationToken = default)
    {
        System.DateTime now = System.DateTime.UtcNow;
        string fromStr = now.AddMinutes(-minutes)
            .ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        string toStr = now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

        return await _db.GnssTrackRecords
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId
                        && string.Compare(x.TrackTime, fromStr, StringComparison.Ordinal) >= 0
                        && string.Compare(x.TrackTime, toStr, StringComparison.Ordinal) <= 0)
            .OrderByDescending(x => x.TrackTime)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<GnssTrackRecord?> GetLatestByDeviceAsync(
        string deviceId,
        CancellationToken cancellationToken = default)
        => _db.GnssTrackRecords
              .AsNoTracking()
              .Where(x => x.DeviceId == deviceId)
              .OrderByDescending(x => x.TrackTime)
              .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<GnssTrackRecord>> GetLatestAllDevicesAsync(
        CancellationToken cancellationToken = default)
    {
        using var conn = new Microsoft.Data.SqlClient.SqlConnection(
            _db.Database.GetConnectionString());
        return (await conn.QueryAsync<GnssTrackRecord>("""
            SELECT *
            FROM (
                SELECT *, ROW_NUMBER() OVER (PARTITION BY DeviceId ORDER BY TrackTime DESC) AS _rn
                FROM   GnssTrackRecords
            ) t
            WHERE t._rn = 1
            ORDER BY DeviceId
            """)).AsList();
    }

    public async Task<IReadOnlyList<GnssTrackRecord>> GetAllDevicesAndDateAsync(
        string date,
        CancellationToken cancellationToken = default)
    {
        string from = $"{date} 00:00:00";
        string to = $"{date} 23:59:59";

        return await _db.GnssTrackRecords
            .AsNoTracking()
            .Where(x => string.Compare(x.TrackTime, from, StringComparison.Ordinal) >= 0
                        && string.Compare(x.TrackTime, to, StringComparison.Ordinal) <= 0)
            .OrderBy(x => x.DeviceId)
            .ThenBy(x => x.TrackTime)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
