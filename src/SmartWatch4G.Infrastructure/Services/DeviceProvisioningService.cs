using Microsoft.Extensions.Logging;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Interfaces;

namespace SmartWatch4G.Infrastructure.Services;

/// <summary>
/// Pushes the five core Iwown commands to each device.
/// Each command is retried (up to MaxAttempts) until the API returns ReturnCode 0.
/// Settings are persisted locally ONLY after a successful API response.
/// </summary>
public sealed class DeviceProvisioningService : IDeviceProvisioningService
{
    private const int MaxAttempts = 3;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(60);

    private readonly IwownService _iwown;
    private readonly IDeviceSettingsService _settings;
    private readonly IDatabaseService _db;
    private readonly ILogger<DeviceProvisioningService> _logger;

    public DeviceProvisioningService(
        IwownService iwown,
        IDeviceSettingsService settings,
        IDatabaseService db,
        ILogger<DeviceProvisioningService> logger)
    {
        _iwown    = iwown;
        _settings = settings;
        _db       = db;
        _logger   = logger;
    }

    public async Task<DeviceProvisioningReport> ProvisionAllAsync(int? companyId = null)
    {
        var profiles = companyId.HasValue
            ? await _db.GetUsersByCompanyId(companyId.Value)
            : await _db.GetAllUserProfiles();

        var activeDevices = profiles
            .Where(p => p.IsActive)
            .Select(p => p.DeviceId)
            .ToList();

        _logger.LogInformation("[Provisioning] Starting for {Count} devices", activeDevices.Count);

        var results = new List<DeviceProvisioningResult>();
        foreach (var deviceId in activeDevices)
            results.Add(await ProvisionDeviceAsync(deviceId));

        var report = new DeviceProvisioningReport
        {
            Total     = results.Count,
            Succeeded = results.Count(r => r.Success),
            Failed    = results.Count(r => !r.Success),
            Results   = results
        };

        _logger.LogInformation("[Provisioning] Complete — {Succeeded}/{Total} succeeded",
            report.Succeeded, report.Total);

        return report;
    }

    public async Task<DeviceProvisioningResult> ProvisionDeviceAsync(string deviceId)
    {
        var errors = new List<string>();

        _logger.LogInformation("[Provisioning] Device {DeviceId} — starting", deviceId);

        // Load whatever is already in the settings tables so we can skip commands
        // that were already successfully provisioned on a previous run.
        var config = await _db.GetDeviceConfig(deviceId);

        // ── 1. Set Data Frequency ──────────────────────────────────────────────
        // POST /entservice/cmd/datafreq
        // Skip if device_data_freq row already exists (GpsAutoCheck is set).
        if (config?.GpsAutoCheck != null)
        {
            _logger.LogInformation("[Provisioning] Device {DeviceId} — cmd/datafreq already provisioned, skipping", deviceId);
        }
        else
        {
            await RetryUntilSuccessAsync(deviceId, "cmd/datafreq", errors, async () =>
            {
                var req = new DataFreqRequest
                {
                    device_id         = deviceId,
                    gps_auto_check    = true,
                    gps_interval_time = 60,
                    power_mode        = 2
                };
                var res = await _iwown.SetDataFreqAsync(req);
                if (res?.ReturnCode != 0) return false;
                await _settings.SaveDataFreq(req);
                return true;
            });
        }

        // ── 2. Start GPS Locate ────────────────────────────────────────────────
        // POST /entservice/cmd/gps/locate
        // Skip if device_gps_settings row already exists (GpsLocateAutoCheck is set).
        if (config?.GpsLocateAutoCheck != null)
        {
            _logger.LogInformation("[Provisioning] Device {DeviceId} — cmd/gps/locate already provisioned, skipping", deviceId);
        }
        else
        {
            await RetryUntilSuccessAsync(deviceId, "cmd/gps/locate", errors, async () =>
            {
                var req = new GpsLocateRequest
                {
                    device_id         = deviceId,
                    gps_auto_check    = true,
                    gps_interval_time = 60,
                    run_gps           = true
                };
                var res = await _iwown.GpsLocateAsync(req);
                if (res?.ReturnCode != 0) return false;
                await _settings.SaveGpsLocate(req);
                return true;
            });
        }

        // ── 3. Trigger Data Sync ───────────────────────────────────────────────
        // POST /entservice/cmd/datasync
        await RetryUntilSuccessAsync(deviceId, "cmd/datasync", errors, async () =>
        {
            var res = await _iwown.RequestDataSyncAsync(new DeviceIdRequest { device_id = deviceId });
            return res?.Succeeded == true;
        });

        // ── 4. Get Real-Time Location ──────────────────────────────────────────
        // POST /entservice/cmd/realtime/location
        await RetryUntilSuccessAsync(deviceId, "cmd/realtime/location", errors, async () =>
        {
            var res = await _iwown.EnableRealtimeLocationAsync(new DeviceIdRequest { device_id = deviceId });
            return res?.Succeeded == true;
        });

        // ── 5. Get Device Status ───────────────────────────────────────────────
        // GET /entservice/device/status?device_id=...
        await RetryUntilSuccessAsync(deviceId, "device/status", errors, async () =>
        {
            var res = await _iwown.GetDeviceStatusAsync(deviceId);
            return res?.Succeeded == true;
        });

        var success = errors.Count == 0;
        if (success)
            _logger.LogInformation("[Provisioning] Device {DeviceId} fully provisioned", deviceId);
        else
            _logger.LogWarning("[Provisioning] Device {DeviceId} finished with {Count} failed command(s)", deviceId, errors.Count);

        return new DeviceProvisioningResult
        {
            DeviceId = deviceId,
            Success  = success,
            Errors   = errors
        };
    }

    // ── Retry helper ──────────────────────────────────────────────────────────

    /// <summary>
    /// Calls <paramref name="command"/> repeatedly until it returns <c>true</c>
    /// (API ReturnCode == 0) or <see cref="MaxAttempts"/> is exhausted.
    /// Nothing is saved locally unless the lambda itself returns true.
    /// </summary>
    private async Task RetryUntilSuccessAsync(
        string deviceId,
        string commandName,
        List<string> errors,
        Func<Task<bool>> command)
    {
        for (int attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                bool ok = await command();
                if (ok)
                {
                    _logger.LogInformation(
                        "[Provisioning] {DeviceId} ✓ {Command} (attempt {Attempt})",
                        deviceId, commandName, attempt);
                    return;
                }

                _logger.LogWarning(
                    "[Provisioning] {DeviceId} ✗ {Command} — non-zero ReturnCode (attempt {Attempt}/{Max}), retrying in {Delay}s…",
                    deviceId, commandName, attempt, MaxAttempts, RetryDelay.TotalSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "[Provisioning] {DeviceId} ✗ {Command} — exception (attempt {Attempt}/{Max}), retrying in {Delay}s…",
                    deviceId, commandName, attempt, MaxAttempts, RetryDelay.TotalSeconds);
            }

            if (attempt < MaxAttempts)
                await Task.Delay(RetryDelay);
        }

        var msg = $"{commandName}: failed after {MaxAttempts} attempts";
        errors.Add(msg);
        _logger.LogError("[Provisioning] {DeviceId} ✗ {Command} — giving up after {Max} attempts",
            deviceId, commandName, MaxAttempts);
    }
}
