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
