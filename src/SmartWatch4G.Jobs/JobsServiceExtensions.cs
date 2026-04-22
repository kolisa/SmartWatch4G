using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace SmartWatch4G.Jobs;

public static class JobsServiceExtensions
{
    /// <summary>
    /// Registers the Quartz scheduler, the log-file monitor singleton worker,
    /// and the 2-second fallback polling job.
    /// Call this from <c>KhoiWatchData.Api/Program.cs</c>.
    /// </summary>
    public static IServiceCollection AddJobs(this IServiceCollection services)
    {
        // Singleton worker owns the FileSystemWatcher + state — must outlive jobs
        services.AddSingleton<LogFileMonitorWorker>();

        services.AddQuartz(q =>
        {
            var jobKey = new JobKey("LogFileMonitorJob");

            q.AddJob<LogFileMonitorJob>(opts => opts.WithIdentity(jobKey));

            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("LogFileMonitorJob-trigger")
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(2)
                    .RepeatForever()));
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }
}
