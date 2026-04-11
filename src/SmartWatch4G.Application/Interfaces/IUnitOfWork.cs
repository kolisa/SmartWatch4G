namespace SmartWatch4G.Application.Interfaces;

/// <summary>
/// Abstracts the database unit-of-work boundary.
/// Repositories only <em>track</em> changes; callers commit them by calling
/// <see cref="CommitAsync"/> once at the end of the logical operation.
/// This enables batching multiple inserts into a single round-trip to SQL
/// Server, which is critical at &gt;100 000-device scale.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Flushes all pending change-tracker entries to the database in a single
    /// network round-trip.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);
}
