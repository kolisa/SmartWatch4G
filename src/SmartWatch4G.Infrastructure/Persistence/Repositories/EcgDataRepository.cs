using Microsoft.EntityFrameworkCore;

using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Persistence.Repositories;

internal sealed class EcgDataRepository : IEcgDataRepository
{
    private readonly AppDbContext _db;

    public EcgDataRepository(AppDbContext db) => _db = db;

    public Task AddAsync(EcgDataRecord record, CancellationToken cancellationToken = default)
    {
        _db.EcgDataRecords.Add(record);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<EcgDataRecord>> GetByDeviceAndDateAsync(
        string deviceId,
        string date,
        CancellationToken cancellationToken = default)
    {
        string from = $"{date} 00:00:00";
        string to = $"{date} 23:59:59";

        return await _db.EcgDataRecords
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId
                        && string.Compare(x.DataTime, from, StringComparison.Ordinal) >= 0
                        && string.Compare(x.DataTime, to, StringComparison.Ordinal) <= 0)
            .OrderBy(x => x.Seq)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
