using Dapper;

using Microsoft.EntityFrameworkCore;

using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Persistence.Repositories;

internal sealed class DeviceStatusRepository : IDeviceStatusRepository
{
    private readonly AppDbContext _db;

    public DeviceStatusRepository(AppDbContext db) => _db = db;

    public Task AddAsync(DeviceStatusRecord record, CancellationToken cancellationToken = default)
    {
        _db.DeviceStatusRecords.Add(record);
        return Task.CompletedTask;
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
        using var conn = new Microsoft.Data.SqlClient.SqlConnection(
            _db.Database.GetConnectionString());
        return (await conn.QueryAsync<DeviceStatusRecord>("""
            SELECT *
            FROM (
                SELECT *, ROW_NUMBER() OVER (PARTITION BY DeviceId ORDER BY ReceivedAt DESC) AS _rn
                FROM   DeviceStatusRecords
            ) t
            WHERE t._rn = 1
            ORDER BY DeviceId
            """)).AsList();
    }
}
