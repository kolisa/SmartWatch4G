using SmartWatch4G.Domain.Entities;

namespace SmartWatch4G.Domain.Interfaces.Repositories;

public interface ICallLogRepository
{
    Task AddRangeAsync(IEnumerable<CallLogRecord> records, CancellationToken cancellationToken = default);
}
