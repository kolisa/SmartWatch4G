using SmartWatch4G.Domain.Common;

namespace SmartWatch4G.Domain.Interfaces.Services;

/// <summary>
/// Retrieves computed sleep results for a device and date.
/// </summary>
public interface ISleepQueryService
{
    /// <summary>
    /// Returns the sleep analysis for a single date.
    /// A success result with a <c>null</c> value means no sleep data exists for that date.
    /// A failure result means the input was invalid or an unrecoverable error occurred.
    /// </summary>
    Task<ServiceResult<SleepResult?>> GetSleepResultAsync(
        string deviceId,
        string sleepDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns sleep results for each day in the inclusive date range.
    /// Days with no data are omitted from the list.
    /// Returns a failure result when the date range is invalid.
    /// </summary>
    Task<ServiceResult<IReadOnlyList<SleepResult>>> GetSleepResultsByDateRangeAsync(
        string deviceId,
        string fromDate,
        string toDate,
        CancellationToken cancellationToken = default);
}

/// <summary>Sleep result returned to the API consumer.</summary>
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
