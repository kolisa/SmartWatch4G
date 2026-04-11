using Microsoft.EntityFrameworkCore;

using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Persistence.Repositories;

internal sealed class AccDataRepository : IAccDataRepository
{
    private readonly AppDbContext _db;

    public AccDataRepository(AppDbContext db) => _db = db;

    public Task AddAsync(AccDataRecord record, CancellationToken cancellationToken = default)
    {
        _db.AccDataRecords.Add(record);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<AccDataRecord>> GetByDeviceAndDateRangeAsync(
        string deviceId,
        string fromTime,
        string toTime,
        CancellationToken cancellationToken = default)
    {
        return await _db.AccDataRecords
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId
                        && string.Compare(x.DataTime, fromTime, StringComparison.Ordinal) >= 0
                        && string.Compare(x.DataTime, toTime, StringComparison.Ordinal) <= 0)
            .OrderBy(x => x.DataTime)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
