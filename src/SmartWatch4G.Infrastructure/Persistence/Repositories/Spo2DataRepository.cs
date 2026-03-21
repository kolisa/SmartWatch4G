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
}
