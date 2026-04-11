using Microsoft.EntityFrameworkCore;

using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Persistence.Repositories;

internal sealed class PpgDataRepository : IPpgDataRepository
{
    private readonly AppDbContext _db;

    public PpgDataRepository(AppDbContext db) => _db = db;

    public Task AddAsync(PpgDataRecord record, CancellationToken cancellationToken = default)
    {
        _db.PpgDataRecords.Add(record);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<PpgDataRecord>> GetByDeviceAndDateAsync(
        string deviceId,
        string date,
        CancellationToken cancellationToken = default)
    {
        string from = $"{date} 00:00:00";
        string to = $"{date} 23:59:59";

        return await _db.PpgDataRecords
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId
                        && string.Compare(x.DataTime, from, StringComparison.Ordinal) >= 0
                        && string.Compare(x.DataTime, to, StringComparison.Ordinal) <= 0)
            .OrderBy(x => x.DataTime)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
