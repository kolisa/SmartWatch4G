using Microsoft.EntityFrameworkCore;

using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Persistence.Repositories;

internal sealed class SleepDataRepository : ISleepDataRepository
{
    private readonly AppDbContext _db;

    public SleepDataRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(SleepDataRecord record, CancellationToken cancellationToken = default)
    {
        _db.SleepDataRecords.Add(record);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<SleepDataRecord>> GetByDeviceAndDateAsync(
        string deviceId,
        string sleepDate,
        CancellationToken cancellationToken = default)
    {
        return await _db.SleepDataRecords
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId && x.SleepDate == sleepDate)
            .OrderBy(x => x.Seq)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
