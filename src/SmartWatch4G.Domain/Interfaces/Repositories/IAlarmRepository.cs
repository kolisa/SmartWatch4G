using SmartWatch4G.Domain.Entities;

namespace SmartWatch4G.Domain.Interfaces.Repositories;

public interface IAlarmRepository
{
    Task AddRangeAsync(IEnumerable<AlarmEventRecord> records, CancellationToken cancellationToken = default);
}
