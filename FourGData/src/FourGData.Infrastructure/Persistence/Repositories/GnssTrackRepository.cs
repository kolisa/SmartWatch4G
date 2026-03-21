using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using SmartWatch4G.Infrastructure.Persistence;

namespace SmartWatch4G.Infrastructure.Persistence.Repositories;

internal sealed class GnssTrackRepository : IGnssTrackRepository
{
    private readonly AppDbContext _db;

    public GnssTrackRepository(AppDbContext db) => _db = db;

    public async Task AddRangeAsync(IEnumerable<GnssTrackRecord> records, CancellationToken cancellationToken = default)
    {
        _db.GnssTrackRecords.AddRange(records);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
