using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using SmartWatch4G.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SmartWatch4G.Infrastructure.Persistence.Repositories;

internal sealed class DeviceInfoRepository : IDeviceInfoRepository
{
    private readonly AppDbContext _db;

    public DeviceInfoRepository(AppDbContext db) => _db = db;

    public async Task UpsertAsync(DeviceInfoRecord record, CancellationToken cancellationToken = default)
    {
        DeviceInfoRecord? existing = await _db.DeviceInfoRecords
            .FirstOrDefaultAsync(x => x.DeviceId == record.DeviceId, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            _db.DeviceInfoRecords.Add(record);
        }
        else
        {
            existing.Imsi = record.Imsi;
            existing.Sn = record.Sn;
            existing.Mac = record.Mac;
            existing.NetType = record.NetType;
            existing.NetOperator = record.NetOperator;
            existing.WearingStatus = record.WearingStatus;
            existing.Model = record.Model;
            existing.Version = record.Version;
            existing.Sim1IccId = record.Sim1IccId;
            existing.Sim1CellId = record.Sim1CellId;
            existing.Sim1NetAdhere = record.Sim1NetAdhere;
            existing.NetworkStatus = record.NetworkStatus;
            existing.BandDetail = record.BandDetail;
            existing.RefSignal = record.RefSignal;
            existing.Band = record.Band;
            existing.CommunicationMode = record.CommunicationMode;
            existing.WatchEvent = record.WatchEvent;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task<DeviceInfoRecord?> FindByDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default)
        => _db.DeviceInfoRecords
               .AsNoTracking()
               .FirstOrDefaultAsync(x => x.DeviceId == deviceId, cancellationToken);
}
