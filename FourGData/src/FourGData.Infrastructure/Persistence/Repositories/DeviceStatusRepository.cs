using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using SmartWatch4G.Infrastructure.Persistence;

namespace SmartWatch4G.Infrastructure.Persistence.Repositories;

internal sealed class DeviceStatusRepository : IDeviceStatusRepository
{
    private readonly AppDbContext _db;

    public DeviceStatusRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(DeviceStatusRecord record, CancellationToken cancellationToken = default)
    {
        _db.DeviceStatusRecords.Add(record);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
