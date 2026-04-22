using Microsoft.Extensions.Logging;
using Quartz;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Interfaces;
using SmartWatch4G.Infrastructure.Services;

namespace SmartWatch4G.Jobs;

/// <summary>
/// Quartz job that runs every 30 seconds and refreshes the in-memory device status cache
/// by calling the Iwown API for every active device profile.
/// All dashboard/map endpoints read from <see cref="IDeviceStatusCache"/> instead of
/// making per-request API calls, so the map always reflects the last-polled state.
/// </summary>
[DisallowConcurrentExecution]
public sealed class DeviceStatusPollingJob : IJob
{
    private readonly IDatabaseService _db;
    private readonly IwownService _iwown;
    private readonly IDeviceStatusCache _cache;
    private readonly ILogger<DeviceStatusPollingJob> _logger;

    public DeviceStatusPollingJob(
        IDatabaseService db,
        IwownService iwown,
        IDeviceStatusCache cache,
        ILogger<DeviceStatusPollingJob> logger)
    {
        _db     = db;
        _iwown  = iwown;
        _cache  = cache;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var profiles = _db.GetAllUserProfiles();
        if (profiles.Count == 0) return;

        _logger.LogDebug("DeviceStatusPollingJob: polling {Count} devices", profiles.Count);

        // Fan out all status requests in parallel — bounded by the Iwown HTTP client timeout (30s)
        var tasks = profiles.Select(async p =>
        {
            try
            {
                var response = await _iwown.GetDeviceStatusAsync(p.DeviceId);
                var isOnline = DeviceStatusParser.IsOnline(response);
                _cache.SetStatus(p.DeviceId, isOnline);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "DeviceStatusPollingJob: failed to get status for {DeviceId}", p.DeviceId);
                // Keep whatever the cache previously held — do not evict on transient error
            }
        });

        await Task.WhenAll(tasks);

        _logger.LogDebug("DeviceStatusPollingJob: poll complete");
    }
}
