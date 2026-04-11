using Dapper;

using Microsoft.EntityFrameworkCore;

using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

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
            existing.UpdatedAt = System.DateTime.UtcNow;
        }

        // Caller must commit via IUnitOfWork.CommitAsync
    }

    public Task<DeviceInfoRecord?> FindByDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default)
        => _db.DeviceInfoRecords
               .AsNoTracking()
               .FirstOrDefaultAsync(x => x.DeviceId == deviceId, cancellationToken);

    public async Task<IReadOnlyList<DeviceInfoRecord>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Dapper bypasses the EF change tracker — ~2× faster for large read-only
        // result sets. At 100 000+ devices the change-tracker overhead adds up.
        using var conn = new Microsoft.Data.SqlClient.SqlConnection(
            _db.Database.GetConnectionString());
        return (await conn.QueryAsync<DeviceInfoRecord>(
            "SELECT * FROM DeviceInfoRecords ORDER BY DeviceId")).AsList();
    }
}
