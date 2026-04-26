using Quartz;

namespace SmartWatch4G.Jobs;

/// <summary>
/// Quartz fallback job: fires every 2 seconds to catch any lines the
/// <see cref="FileSystemWatcher"/> inside <see cref="LogFileMonitorWorker"/> may
/// have missed (e.g. rapid sequential writes, network shares).
/// The worker's internal lock prevents concurrent processing between this job
/// and the real-time FSW trigger.
/// </summary>
[DisallowConcurrentExecution]
public sealed class LogFileMonitorJob : IJob
{
    private readonly LogFileMonitorWorker _worker;

    public LogFileMonitorJob(LogFileMonitorWorker worker)
    {
        _worker = worker;
    }

    public Task Execute(IJobExecutionContext context) => _worker.Execute();
}
