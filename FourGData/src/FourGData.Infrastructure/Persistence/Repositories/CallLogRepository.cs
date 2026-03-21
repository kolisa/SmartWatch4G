using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using SmartWatch4G.Infrastructure.Persistence;

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
}
