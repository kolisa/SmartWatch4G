using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using SmartWatch4G.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SmartWatch4G.Infrastructure.Persistence.Repositories;

internal sealed class RriDataRepository : IRriDataRepository
{
    private readonly AppDbContext _db;

    public RriDataRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(RriDataRecord record, CancellationToken cancellationToken = default)
    {
        _db.RriDataRecords.Add(record);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<RriDataRecord>> GetByDeviceAndDateAsync(
        string deviceId,
        string date,
        CancellationToken cancellationToken = default)
    {
        // DataTime format is "yyyy-MM-dd HH:mm:ss" — prefix match on the date portion
        string prefix = date + " ";
        return await _db.RriDataRecords
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId && x.DataTime.StartsWith(prefix))
            .OrderBy(x => x.DataTime)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
