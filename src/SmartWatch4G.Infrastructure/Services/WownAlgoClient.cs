using System.Net.Http.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SmartWatch4G.Domain.Interfaces.Services;

namespace SmartWatch4G.Infrastructure.Services;

/// <summary>
/// HTTP client for the iwown algorithm service.
/// China Mainland : https://api1.iwown.com/algoservice
/// Rest of world  : https://iwap1.iwown.com/algoservice
/// </summary>
public sealed class WownAlgoClient : IWownAlgoClient
{
    private readonly HttpClient _http;
    private readonly WownAlgoOptions _options;
    private readonly ILogger<WownAlgoClient> _logger;

    public WownAlgoClient(
        HttpClient http,
        IOptions<WownAlgoOptions> options,
        ILogger<WownAlgoClient> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<SleepCalcResult?> CalculateSleepAsync(
        SleepCalcRequest request,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            prevDay = request.PrevDaySleepJson,
            nextDay = request.NextDaySleepJson,
            prevDayRri = request.PrevDayRriList,
            nextDayRri = request.NextDayRriList,
            recordDate = request.RecordDate,
            device_id = request.DeviceId,
            account = _options.Account,
            password = _options.Password
        };

        AlgoResponse<SleepAlgoData>? response;
        try
        {
            var httpResp = await _http.PostAsJsonAsync(
                "/calculation/sleep", body, cancellationToken).ConfigureAwait(false);

            httpResp.EnsureSuccessStatusCode();
            response = await httpResp.Content
                .ReadFromJsonAsync<AlgoResponse<SleepAlgoData>>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError("iwown sleep algo call failed: {Message}", ex.Message);
            return null;
        }

        if (response is null || response.ReturnCode != 0 || response.Data is null)
        {
            _logger.LogWarning(
                "iwown sleep algo returned code {Code}: {Msg}",
                response?.ReturnCode, response?.Message);
            return null;
        }

        return new SleepCalcResult
        {
            StartTime = response.Data.StartTime ?? string.Empty,
            EndTime = response.Data.EndTime ?? string.Empty,
            HeartRate = response.Data.Hr,
            TurnTimes = response.Data.TurnTimes,
            Sections = response.Data.Sections?
                .Select(s => new SleepSection
                {
                    Start = s.Start ?? string.Empty,
                    End = s.End ?? string.Empty,
                    Type = s.Type
                })
                .ToList() ?? []
        };
    }

    /// <inheritdoc/>
    public async Task<RhythmCalcResult?> CalculateEcgAsync(
        EcgCalcRequest request,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            ecg_list = request.EcgList,
            device_id = request.DeviceId,
            account = _options.Account,
            password = _options.Password
        };

        AlgoResponse<EcgAlgoData>? response;
        try
        {
            var httpResp = await _http.PostAsJsonAsync(
                "/calculation/ecg", body, cancellationToken).ConfigureAwait(false);
            httpResp.EnsureSuccessStatusCode();
            response = await httpResp.Content
                .ReadFromJsonAsync<AlgoResponse<EcgAlgoData>>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError("iwown ECG algo call failed: {Message}", ex.Message);
            return null;
        }

        if (response is null || response.ReturnCode != 0 || response.Data is null)
        {
            _logger.LogWarning("iwown ECG algo returned code {Code}: {Msg}", response?.ReturnCode, response?.Message);
            return null;
        }

        return new RhythmCalcResult
        {
            Result = response.Data.Result,
            HeartRate = response.Data.Hr,
            Effective = response.Data.Effective,
            Direction = response.Data.Direction
        };
    }

    /// <inheritdoc/>
    public async Task<RhythmCalcResult?> CalculateAfAsync(
        AfCalcRequest request,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            rri_list = request.RriList,
            device_id = request.DeviceId,
            account = _options.Account,
            password = _options.Password
        };

        AlgoResponse<AfAlgoData>? response;
        try
        {
            var httpResp = await _http.PostAsJsonAsync(
                "/calculation/af", body, cancellationToken).ConfigureAwait(false);
            httpResp.EnsureSuccessStatusCode();
            response = await httpResp.Content
                .ReadFromJsonAsync<AlgoResponse<AfAlgoData>>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError("iwown AF algo call failed: {Message}", ex.Message);
            return null;
        }

        if (response is null || response.ReturnCode != 0 || response.Data is null)
        {
            _logger.LogWarning("iwown AF algo returned code {Code}: {Msg}", response?.ReturnCode, response?.Message);
            return null;
        }

        return new RhythmCalcResult { Result = response.Data.Result };
    }

    /// <inheritdoc/>
    public async Task<Spo2CalcResult?> CalculateSpo2Async(
        Spo2CalcRequest request,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            spo2_list = request.Spo2List,
            device_id = request.DeviceId,
            account = _options.Account,
            password = _options.Password
        };

        AlgoResponse<Spo2AlgoData>? response;
        try
        {
            var httpResp = await _http.PostAsJsonAsync(
                "/calculation/spo2", body, cancellationToken).ConfigureAwait(false);
            httpResp.EnsureSuccessStatusCode();
            response = await httpResp.Content
                .ReadFromJsonAsync<AlgoResponse<Spo2AlgoData>>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError("iwown SpO2 algo call failed: {Message}", ex.Message);
            return null;
        }

        if (response is null || response.ReturnCode != 0 || response.Data is null)
        {
            _logger.LogWarning("iwown SpO2 algo returned code {Code}: {Msg}", response?.ReturnCode, response?.Message);
            return null;
        }

        return new Spo2CalcResult
        {
            Spo2Score = response.Data.Spo2Score,
            OsahsRisk = response.Data.OsahsRisk
        };
    }

    /// <inheritdoc/>
    public async Task<ParkinsonCalcResult?> CalculateParkinsonAsync(
        ParkinsonCalcRequest request,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            acc_x = request.AccXList,
            acc_y = request.AccYList,
            acc_z = request.AccZList,
            device_id = request.DeviceId,
            account = _options.Account,
            password = _options.Password
        };

        AlgoResponse<ParkinsonAlgoData>? response;
        try
        {
            var httpResp = await _http.PostAsJsonAsync(
                "/calculation/parkinson/acc", body, cancellationToken).ConfigureAwait(false);
            httpResp.EnsureSuccessStatusCode();
            response = await httpResp.Content
                .ReadFromJsonAsync<AlgoResponse<ParkinsonAlgoData>>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError("iwown Parkinson algo call failed: {Message}", ex.Message);
            return null;
        }

        if (response is null || response.ReturnCode != 0 || response.Data is null)
        {
            _logger.LogWarning("iwown Parkinson algo returned code {Code}: {Msg}", response?.ReturnCode, response?.Message);
            return null;
        }

        return new ParkinsonCalcResult
        {
            TremorScore = response.Data.TremorScore,
            ActivityScore = response.Data.ActivityScore
        };
    }

    // ── Private DTOs (API contract; not part of the domain) ──────────────────

    private sealed class AlgoResponse<T>
    {
        [JsonPropertyName("ReturnCode")]
        public int ReturnCode { get; init; }

        [JsonPropertyName("message")]
        public string? Message { get; init; }

        [JsonPropertyName("data")]
        public T? Data { get; init; }
    }

    private sealed class SleepAlgoData
    {
        [JsonPropertyName("start_time")]
        public string? StartTime { get; init; }

        [JsonPropertyName("end_time")]
        public string? EndTime { get; init; }

        [JsonPropertyName("hr")]
        public int Hr { get; init; }

        [JsonPropertyName("turn_times")]
        public int TurnTimes { get; init; }

        [JsonPropertyName("sections")]
        public List<SleepSectionDto>? Sections { get; init; }
    }

    private sealed class SleepSectionDto
    {
        [JsonPropertyName("start")]
        public string? Start { get; init; }

        [JsonPropertyName("end")]
        public string? End { get; init; }

        [JsonPropertyName("type")]
        public int Type { get; init; }
    }

    private sealed class EcgAlgoData
    {
        [JsonPropertyName("result")]
        public int Result { get; init; }

        [JsonPropertyName("hr")]
        public int Hr { get; init; }

        [JsonPropertyName("effective")]
        public int Effective { get; init; }

        [JsonPropertyName("direction")]
        public int Direction { get; init; }
    }

    private sealed class AfAlgoData
    {
        [JsonPropertyName("result")]
        public int Result { get; init; }
    }

    private sealed class Spo2AlgoData
    {
        [JsonPropertyName("spo2_score")]
        public int Spo2Score { get; init; }

        [JsonPropertyName("osahs_risk")]
        public int OsahsRisk { get; init; }
    }

    private sealed class ParkinsonAlgoData
    {
        [JsonPropertyName("tremor_score")]
        public int TremorScore { get; init; }

        [JsonPropertyName("activity_score")]
        public int ActivityScore { get; init; }
    }
}

/// <summary>
/// Configuration for the iwown algorithm service (bind from appsettings.json).
/// </summary>
public sealed class WownAlgoOptions
{
    public const string SectionName = "WownAlgo";

    /// <summary>Base URL — use https://api1.iwown.com/algoservice (China) or
    /// https://iwap1.iwown.com/algoservice (rest of world).</summary>
    public string BaseUrl { get; set; } = "https://api1.iwown.com/algoservice";

    public string Account { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
