namespace SmartWatch4G.Application.Interfaces;

/// <summary>
/// In-memory cache for the last-known online/offline status of each device.
/// Updated by the background polling job every 30 seconds.
/// </summary>
public interface IDeviceStatusCache
{
    /// <summary>Returns "online" or "offline". Returns "unknown" if the device has not been polled yet.</summary>
    string GetStatus(string deviceId);

    bool IsOnline(string deviceId);

    /// <summary>Writes the latest status for a device. Called only by the polling job.</summary>
    void SetStatus(string deviceId, bool isOnline);

    /// <summary>Returns all device IDs currently tracked in the cache.</summary>
    IReadOnlyList<string> GetAllDeviceIds();
}
