using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using SmartWatch4G.Application.Interfaces;

namespace SmartWatch4G.Jobs;

/// <summary>
/// Quartz job that provisions all active devices via the Iwown command API.
/// Scheduled to run 3× daily (06:00, 12:00, 18:00 UTC).
/// Each command is retried up to 3 times with a 60-second delay before being marked as failed.
/// Settings are persisted locally only after the API confirms success (ReturnCode == 0).
/// </summary>
[DisallowConcurrentExecution]
public sealed class DeviceProvisioningJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DeviceProvisioningJob> _logger;

    public DeviceProvisioningJob(IServiceScopeFactory scopeFactory, ILogger<DeviceProvisioningJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("[DeviceProvisioningJob] Run started at {Time}", DateTimeOffset.UtcNow);

        // IDeviceProvisioningService is scoped — resolve inside a fresh scope per run
        await using var scope = _scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IDeviceProvisioningService>();

        var report = await service.ProvisionAllAsync();

        _logger.LogInformation(
            "[DeviceProvisioningJob] Run complete — {Succeeded}/{Total} devices provisioned successfully",
            report.Succeeded, report.Total);

        if (report.Failed > 0)
        {
            foreach (var result in report.Results.Where(r => !r.Success))
            {
                _logger.LogWarning(
                    "[DeviceProvisioningJob] Device {DeviceId} failed: {Errors}",
                    result.DeviceId, string.Join("; ", result.Errors));
            }
        }
    }
}
