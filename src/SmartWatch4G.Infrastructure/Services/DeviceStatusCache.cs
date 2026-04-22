using System.Collections.Concurrent;
using SmartWatch4G.Application.Interfaces;

namespace SmartWatch4G.Infrastructure.Services;

/// <summary>
/// Thread-safe singleton that holds the last-known online/offline status for every device.
/// Populated by <see cref="SmartWatch4G.Jobs.DeviceStatusPollingJob"/> every 30 seconds.
/// All API endpoints read from this cache — no per-request calls to the Iwown API are needed.
/// </summary>
public sealed class DeviceStatusCache : IDeviceStatusCache
{
    // deviceId → isOnline
    private readonly ConcurrentDictionary<string, bool> _cache = new(StringComparer.OrdinalIgnoreCase);

    public string GetStatus(string deviceId)
    {
        if (!_cache.TryGetValue(deviceId, out var isOnline))
            return "unknown";

        return isOnline ? "online" : "offline";
    }

    public bool IsOnline(string deviceId) =>
        _cache.TryGetValue(deviceId, out var online) && online;

    public void SetStatus(string deviceId, bool isOnline) =>
        _cache[deviceId] = isOnline;

    public IReadOnlyList<string> GetAllDeviceIds() =>
        _cache.Keys.ToList();
}
