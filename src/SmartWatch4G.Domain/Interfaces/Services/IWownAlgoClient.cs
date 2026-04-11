namespace SmartWatch4G.Domain.Interfaces.Services;

/// <summary>
/// Client for the iwown algorithm service.
/// China Mainland: https://api1.iwown.com/algoservice
/// Rest of world:  https://iwap1.iwown.com/algoservice
/// </summary>
public interface IWownAlgoClient
{
    /// <summary>Calculates sleep stages for the given record date.</summary>
    Task<SleepCalcResult?> CalculateSleepAsync(SleepCalcRequest request, CancellationToken cancellationToken = default);

    /// <summary>Classifies ECG rhythm from raw sample data (<c>POST /calculation/ecg</c>).</summary>
    Task<RhythmCalcResult?> CalculateEcgAsync(EcgCalcRequest request, CancellationToken cancellationToken = default);

    /// <summary>Detects AF / arrhythmia from RRI sequences (<c>POST /calculation/af</c>).</summary>
    Task<RhythmCalcResult?> CalculateAfAsync(AfCalcRequest request, CancellationToken cancellationToken = default);

    /// <summary>Calculates OSAHS risk from continuous SpO2 samples (<c>POST /calculation/spo2</c>).</summary>
    Task<Spo2CalcResult?> CalculateSpo2Async(Spo2CalcRequest request, CancellationToken cancellationToken = default);

    /// <summary>Calculates Parkinson tremor/activity score from ACC data (<c>POST /calculation/parkinson/acc</c>).</summary>
    Task<ParkinsonCalcResult?> CalculateParkinsonAsync(ParkinsonCalcRequest request, CancellationToken cancellationToken = default);
}

// ── Sleep ──────────────────────────────────────────────────────────────────────

/// <summary>Input for <c>POST /calculation/sleep</c>.</summary>
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

    /// <summary>3=Deep, 4=Light, 6=Awake, 7=REM.</summary>
    public int Type { get; init; }
}

// ── ECG ────────────────────────────────────────────────────────────────────────

/// <summary>Input for <c>POST /calculation/ecg</c>.</summary>
public sealed record EcgCalcRequest(
    IReadOnlyList<int> EcgList,
    string DeviceId);

// ── AF ─────────────────────────────────────────────────────────────────────────

/// <summary>Input for <c>POST /calculation/af</c>.</summary>
public sealed record AfCalcRequest(
    IReadOnlyList<long> RriList,
    string DeviceId);

/// <summary>Result for ECG or AF rhythm classification.</summary>
public sealed class RhythmCalcResult
{
    /// <summary>
    /// 0=No result/interference, 1=Sinus, 2=Brady, 3=Tachy,
    /// 4=Premature beats, 5=AF, 6=SVT.
    /// </summary>
    public int Result { get; init; }

    /// <summary>Heart rate bpm (ECG only).</summary>
    public int HeartRate { get; init; }

    /// <summary>0=effective, -1=weak signal, 1=interference (ECG only).</summary>
    public int Effective { get; init; }

    /// <summary>-1=reversed, 0=normal (ECG only).</summary>
    public int Direction { get; init; }
}

// ── SPO2 ───────────────────────────────────────────────────────────────────────

/// <summary>Input for <c>POST /calculation/spo2</c>.</summary>
public sealed record Spo2CalcRequest(
    IReadOnlyList<int> Spo2List,
    string DeviceId);

/// <summary>Result of a continuous SpO2 OSAHS-risk analysis.</summary>
public sealed class Spo2CalcResult
{
    public int Spo2Score { get; init; }
    public int OsahsRisk { get; init; }
}

// ── Parkinson ──────────────────────────────────────────────────────────────────

/// <summary>Input for <c>POST /calculation/parkinson/acc</c>.</summary>
public sealed record ParkinsonCalcRequest(
    IReadOnlyList<int> AccXList,
    IReadOnlyList<int> AccYList,
    IReadOnlyList<int> AccZList,
    string DeviceId);

/// <summary>Result of Parkinson tremor/activity analysis.</summary>
public sealed class ParkinsonCalcResult
{
    public int TremorScore { get; init; }
    public int ActivityScore { get; init; }
}
