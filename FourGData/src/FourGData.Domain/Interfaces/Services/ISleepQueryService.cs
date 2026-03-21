namespace SmartWatch4G.Domain.Interfaces.Services;

/// <summary>
/// Retrieves computed sleep results for a device and date.
/// </summary>
public interface ISleepQueryService
{
    Task<SleepResult?> GetSleepResultAsync(string deviceId, string sleepDate, CancellationToken cancellationToken = default);
}

/// <summary>
/// Sleep result returned to the API consumer.
/// </summary>
public sealed record SleepResult(
    string DeviceId,
    string SleepDate,
    string StartTime,
    string EndTime,
    int DeepSleepMinutes,
    int LightSleepMinutes,
    int WeakSleepMinutes,
    int EyeMoveSleepMinutes,
    int Score,
    int OsahsRisk,
    int Spo2Score,
    int SleepHeartRate);
