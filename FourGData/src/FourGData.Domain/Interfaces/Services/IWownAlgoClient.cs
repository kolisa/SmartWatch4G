namespace SmartWatch4G.Domain.Interfaces.Services;

/// <summary>
/// Client for the iwown algorithm service.
/// China Mainland: https://api1.iwown.com/algoservice
/// Rest of world:  https://iwap1.iwown.com/algoservice
/// </summary>
public interface IWownAlgoClient
{
    /// <summary>
    /// Calculates sleep stages for the given record date.
    /// </summary>
    /// <param name="request">All required pre-processed inputs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Sleep sections, or <c>null</c> if no data is available.</returns>
    Task<SleepCalcResult?> CalculateSleepAsync(SleepCalcRequest request, CancellationToken cancellationToken = default);
}

// ── Request / response records ─────────────────────────────────────────────

/// <summary>
/// Input for <c>POST /calculation/sleep</c>.
/// </summary>
/// <param name="PrevDaySleepJson">
/// JSON-array string of sleep slots for the day before <see cref="RecordDate"/>.
/// Example: "[{\"E\":{\"a\":[0,0,22,0]},\"Q\":363,\"T\":[9,2]}]"
/// </param>
/// <param name="NextDaySleepJson">
/// JSON-array string of sleep slots for <see cref="RecordDate"/> itself.
/// </param>
/// <param name="PrevDayRriList">RRI values (ms) for the day before <see cref="RecordDate"/>.</param>
/// <param name="NextDayRriList">RRI values (ms) for <see cref="RecordDate"/>.</param>
/// <param name="RecordDate">Date as int, e.g. 20240101.</param>
/// <param name="DeviceId">Device identifier.</param>
public sealed record SleepCalcRequest(
    string PrevDaySleepJson,
    string NextDaySleepJson,
    IReadOnlyList<long> PrevDayRriList,
    IReadOnlyList<long> NextDayRriList,
    int RecordDate,
    string DeviceId);

/// <summary>Parsed sleep calculation result.</summary>
public sealed class SleepCalcResult
{
    public string StartTime { get; init; } = string.Empty;
    public string EndTime { get; init; } = string.Empty;
    public int HeartRate { get; init; }
    public int TurnTimes { get; init; }
    public IReadOnlyList<SleepSection> Sections { get; init; } = [];
}

/// <summary>One contiguous sleep stage segment.</summary>
public sealed class SleepSection
{
    public string Start { get; init; } = string.Empty;
    public string End { get; init; } = string.Empty;

    /// <summary>
    /// Sleep type:
    /// 3 = Deep sleep,
    /// 4 = Light sleep,
    /// 6 = Awake,
    /// 7 = REM (rapid eye movement).
    /// </summary>
    public int Type { get; init; }
}
