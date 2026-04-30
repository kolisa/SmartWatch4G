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

    /// <summary>
    /// Returns the count of online and offline devices currently in the cache.
    /// When companyId is null, counts all tracked devices.
    /// When companyId is provided, counts only devices whose IDs are in the company
    /// (requires the caller to supply the device list, or use the overload).
    /// </summary>
    (int Online, int Offline) GetCounts();
}
