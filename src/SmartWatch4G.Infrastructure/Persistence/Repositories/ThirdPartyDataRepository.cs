using Microsoft.EntityFrameworkCore;

using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Persistence.Repositories;

internal sealed class ThirdPartyDataRepository : IThirdPartyDataRepository
{
    private readonly AppDbContext _db;

    public ThirdPartyDataRepository(AppDbContext db) => _db = db;

    public Task AddAsync(ThirdPartyDataRecord record, CancellationToken cancellationToken = default)
    {
        _db.ThirdPartyDataRecords.Add(record);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<ThirdPartyDataRecord>> GetByDeviceAndDateAsync(
        string deviceId,
        string date,
        CancellationToken cancellationToken = default)
    {
        string from = $"{date} 00:00:00";
        string to = $"{date} 23:59:59";

        return await _db.ThirdPartyDataRecords
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId
                        && string.Compare(x.DataTime, from, StringComparison.Ordinal) >= 0
                        && string.Compare(x.DataTime, to, StringComparison.Ordinal) <= 0)
            .OrderBy(x => x.DataTime)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
