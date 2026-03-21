using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using SmartWatch4G.Infrastructure.Persistence;

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
}
