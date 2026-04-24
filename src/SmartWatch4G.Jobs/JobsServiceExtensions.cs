using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace SmartWatch4G.Jobs;

public static class JobsServiceExtensions
{
    /// <summary>
    /// Registers the Quartz scheduler, the log-file monitor singleton worker,
    /// the 2-second fallback polling job, and the 30-second device status polling job.
    /// Call this from <c>KhoiWatchData.Api/Program.cs</c>.
    /// </summary>
    public static IServiceCollection AddJobs(this IServiceCollection services, IConfiguration configuration)
    {
        // Singleton worker owns the FileSystemWatcher + state — must outlive jobs
        services.AddSingleton<LogFileMonitorWorker>();

        var pollingIntervalSeconds = configuration.GetValue("DeviceStatusPolling:IntervalSeconds", 30);

        services.AddQuartz(q =>
        {
            // ── Log-file monitor (2-second fallback polling) ─────────────────
            var logJobKey = new JobKey("LogFileMonitorJob");

            q.AddJob<LogFileMonitorJob>(opts => opts.WithIdentity(logJobKey));

            q.AddTrigger(opts => opts
                .ForJob(logJobKey)
                .WithIdentity("LogFileMonitorJob-trigger")
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(2)
                    .RepeatForever()));

            // ── Device status polling job ─────────────────────────────────────
            var statusJobKey = new JobKey("DeviceStatusPollingJob");

            q.AddJob<DeviceStatusPollingJob>(opts => opts.WithIdentity(statusJobKey));

            q.AddTrigger(opts => opts
                .ForJob(statusJobKey)
                .WithIdentity("DeviceStatusPollingJob-trigger")
                // Run once immediately on startup, then repeat at the configured interval
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(pollingIntervalSeconds)
                    .RepeatForever()));

            // ── Device provisioning job (3× daily: 06:00, 12:00, 18:00) ─────────────────
            var provisionJobKey = new JobKey("DeviceProvisioningJob");

            q.AddJob<DeviceProvisioningJob>(opts => opts.WithIdentity(provisionJobKey));

            q.AddTrigger(opts => opts
                .ForJob(provisionJobKey)
                .WithIdentity("DeviceProvisioningJob-trigger")
                .WithCronSchedule("0 0 6,12,18 * * ?"));
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }
}
