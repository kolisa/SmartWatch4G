using System.Collections.Concurrent;
using SmartWatch4G.Application.Interfaces;

namespace SmartWatch4G.Infrastructure.Services;

/// <summary>
/// Thread-safe singleton that holds the last-known online/offline status for every device.
/// Populated by <see cref="SmartWatch4G.Jobs.DeviceStatusPollingJob"/> every 30 seconds.
/// Entries older than <see cref="MaxAge"/> are treated as offline to prevent stale "online"
/// readings when the polling job fails or the Iwown API returns errors.
/// </summary>
public sealed class DeviceStatusCache : IDeviceStatusCache
{
    private static readonly TimeSpan MaxAge = TimeSpan.FromSeconds(90);

    private readonly record struct Entry(bool IsOnline, System.DateTime UpdatedAt);

    // deviceId → (isOnline, timestamp)
    private readonly ConcurrentDictionary<string, Entry> _cache = new(StringComparer.OrdinalIgnoreCase);

    public string GetStatus(string deviceId)
    {
        if (!_cache.TryGetValue(deviceId, out var entry))
            return "unknown";

        if (System.DateTime.UtcNow - entry.UpdatedAt > MaxAge)
            return "offline";

        return entry.IsOnline ? "online" : "offline";
    }

    public bool IsOnline(string deviceId)
    {
        if (!_cache.TryGetValue(deviceId, out var entry))
            return false;

        if (System.DateTime.UtcNow - entry.UpdatedAt > MaxAge)
            return false;

        return entry.IsOnline;
    }

    public void SetStatus(string deviceId, bool isOnline) =>
        _cache[deviceId] = new Entry(isOnline, System.DateTime.UtcNow);

    public IReadOnlyList<string> GetAllDeviceIds() =>
        _cache.Keys.ToList();
}
