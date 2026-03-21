using SmartWatch4G.Domain.Entities;

namespace SmartWatch4G.Domain.Interfaces.Repositories;

public interface IGnssTrackRepository
{
    Task AddRangeAsync(IEnumerable<GnssTrackRecord> records, CancellationToken cancellationToken = default);
}
