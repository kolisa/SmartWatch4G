using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using SmartWatch4G.Infrastructure.Persistence;

namespace SmartWatch4G.Infrastructure.Persistence.Repositories;

internal sealed class HealthDataRepository : IHealthDataRepository
{
    private readonly AppDbContext _db;

    public HealthDataRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(HealthDataRecord record, CancellationToken cancellationToken = default)
    {
        _db.HealthDataRecords.Add(record);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task AddEcgAsync(EcgDataRecord record, CancellationToken cancellationToken = default)
    {
        _db.EcgDataRecords.Add(record);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
