namespace SmartWatch4G.Application.Interfaces;

public interface IDeviceProvisioningService
{
    /// <summary>
    /// Pushes the default command settings to the Iwown API for every registered
    /// active device, then persists the settings locally.
    /// Returns a summary of successes and failures per device.
    /// </summary>
    Task<DeviceProvisioningReport> ProvisionAllAsync(int? companyId = null);

    /// <summary>
    /// Pushes the default command settings to the Iwown API for a single device.
    /// </summary>
    Task<DeviceProvisioningResult> ProvisionDeviceAsync(string deviceId);
}

public sealed class DeviceProvisioningReport
{
    public int Total { get; init; }
    public int Succeeded { get; init; }
    public int Failed { get; init; }
    public IReadOnlyList<DeviceProvisioningResult> Results { get; init; } = [];
}

public sealed class DeviceProvisioningResult
{
    public string DeviceId { get; init; } = string.Empty;
    public bool Success { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
}
