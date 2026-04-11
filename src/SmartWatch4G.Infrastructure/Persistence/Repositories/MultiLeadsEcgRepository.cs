using Microsoft.EntityFrameworkCore;

using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Persistence.Repositories;

internal sealed class MultiLeadsEcgRepository : IMultiLeadsEcgRepository
{
    private readonly AppDbContext _db;

    public MultiLeadsEcgRepository(AppDbContext db) => _db = db;

    public Task AddAsync(MultiLeadsEcgRecord record, CancellationToken cancellationToken = default)
    {
        _db.MultiLeadsEcgRecords.Add(record);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<MultiLeadsEcgRecord>> GetByDeviceAndDateAsync(
        string deviceId,
        string date,
        CancellationToken cancellationToken = default)
    {
        string from = $"{date} 00:00:00";
        string to = $"{date} 23:59:59";

        return await _db.MultiLeadsEcgRecords
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId
                        && string.Compare(x.DataTime, from, StringComparison.Ordinal) >= 0
                        && string.Compare(x.DataTime, to, StringComparison.Ordinal) <= 0)
            .OrderBy(x => x.Seq)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
