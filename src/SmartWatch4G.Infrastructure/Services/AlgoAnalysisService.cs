using System.Text.Json;

using Microsoft.Extensions.Logging;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Interfaces.Repositories;
using SmartWatch4G.Domain.Interfaces.Services;
namespace SmartWatch4G.Infrastructure.Services;

/// <summary>
/// Orchestrates calls to the iwown algorithm service for ECG, AF, SpO2, and Parkinson analysis
/// by loading pre-processed records from the database and submitting them to the algo API.
/// </summary>
public sealed class AlgoAnalysisService : IAlgoAnalysisService
{
    private readonly IEcgDataRepository _ecgRepo;
    private readonly IRriDataRepository _rriRepo;
    private readonly ISpo2DataRepository _spo2Repo;
    private readonly IAccDataRepository _accRepo;
    private readonly IWownAlgoClient _algoClient;
    private readonly ILogger<AlgoAnalysisService> _logger;
    private readonly IDateTimeService _dt;

    public AlgoAnalysisService(
        IEcgDataRepository ecgRepo,
        IRriDataRepository rriRepo,
        ISpo2DataRepository spo2Repo,
        IAccDataRepository accRepo,
        IWownAlgoClient algoClient,
        ILogger<AlgoAnalysisService> logger,
        IDateTimeService dt)
    {
        _ecgRepo = ecgRepo;
        _rriRepo = rriRepo;
        _spo2Repo = spo2Repo;
        _accRepo = accRepo;
        _algoClient = algoClient;
        _logger = logger;
        _dt = dt;
    }

    // ── ECG ───────────────────────────────────────────────────────────────────

    public async Task<RhythmAnalysisDto?> AnalyseEcgAsync(
        string deviceId, string dataTime, CancellationToken ct = default)
    {
        // Load all ECG chunks for this device/measurement (same DataTime → same measurement)
        string date = dataTime.Length >= 10 ? dataTime[..10] : dataTime;
        var records = await _ecgRepo.GetByDeviceAndDateAsync(deviceId, date, ct)
            .ConfigureAwait(false);

        // Filter to the requested dataTime and merge samples in Seq order
        var chunks = records
            .Where(r => r.DataTime == dataTime)
            .OrderBy(r => r.Seq)
            .ToList();

        if (chunks.Count == 0)
        {
            _logger.LogWarning("AnalyseEcg — no ECG records found for device {D}, time {T}", deviceId, dataTime);
            return null;
        }

        // Decode base64 → signed byte array → int list (each byte is a signed ECG sample)
        var ecgList = new List<int>();
        foreach (var chunk in chunks)
        {
            byte[] raw = Convert.FromBase64String(chunk.RawDataBase64);
            ecgList.AddRange(raw.Select(b => (int)(sbyte)b));
        }

        _logger.LogInformation("AnalyseEcg — calling algo, device {D}, samples {N}", deviceId, ecgList.Count);

        var result = await _algoClient.CalculateEcgAsync(
            new EcgCalcRequest(ecgList, deviceId), ct).ConfigureAwait(false);

        if (result is null) return null;

        return new RhythmAnalysisDto
        {
            DeviceId = deviceId,
            DataTime = dataTime,
            Result = result.Result,
            HeartRate = result.HeartRate,
            Effective = result.Effective,
            Direction = result.Direction
        };
    }

    // ── AF ────────────────────────────────────────────────────────────────────

    public async Task<RhythmAnalysisDto?> AnalyseAfAsync(
        string deviceId, string date, CancellationToken ct = default)
    {
        var records = await _rriRepo.GetByDeviceAndDateAsync(deviceId, date, ct)
            .ConfigureAwait(false);

        if (records.Count == 0)
        {
            _logger.LogWarning("AnalyseAf — no RRI records found for device {D}, date {Date}", deviceId, date);
            return null;
        }

        // Flatten all RRI sequences in time order
        var rriList = new List<long>();
        foreach (var r in records.OrderBy(r => r.DataTime).ThenBy(r => r.Seq))
        {
            var values = JsonSerializer.Deserialize<List<long>>(r.RriValuesJson) ?? [];
            rriList.AddRange(values);
        }

        _logger.LogInformation("AnalyseAf — calling algo, device {D}, RRI count {N}", deviceId, rriList.Count);

        var result = await _algoClient.CalculateAfAsync(
            new AfCalcRequest(rriList, deviceId), ct).ConfigureAwait(false);

        if (result is null) return null;

        return new RhythmAnalysisDto
        {
            DeviceId = deviceId,
            DataTime = date,
            Result = result.Result
        };
    }

    // ── SpO2 ─────────────────────────────────────────────────────────────────

    public async Task<Spo2AnalysisDto?> AnalyseSpo2Async(
        string deviceId, string date, CancellationToken ct = default)
    {
        (string from, string to) = _dt.ToDayRange(date);
        var records = await _spo2Repo.GetByDeviceAndDateRangeAsync(deviceId, from, to, ct)
            .ConfigureAwait(false);

        if (records.Count == 0)
        {
            _logger.LogWarning("AnalyseSpo2 — no SpO2 records found for device {D}, date {Date}", deviceId, date);
            return null;
        }

        var spo2List = records.OrderBy(r => r.DataTime).Select(r => r.Spo2).ToList();

        _logger.LogInformation("AnalyseSpo2 — calling algo, device {D}, samples {N}", deviceId, spo2List.Count);

        var result = await _algoClient.CalculateSpo2Async(
            new Spo2CalcRequest(spo2List, deviceId), ct).ConfigureAwait(false);

        if (result is null) return null;

        return new Spo2AnalysisDto
        {
            DeviceId = deviceId,
            Date = date,
            Spo2Score = result.Spo2Score,
            OsahsRisk = result.OsahsRisk
        };
    }

    // ── Parkinson ─────────────────────────────────────────────────────────────

    public async Task<ParkinsonAnalysisDto?> AnalyseParkinsonAsync(
        string deviceId, string date, CancellationToken ct = default)
    {
        (string from, string to) = _dt.ToDayRange(date);
        var records = await _accRepo.GetByDeviceAndDateRangeAsync(deviceId, from, to, ct)
            .ConfigureAwait(false);

        if (records.Count == 0)
        {
            _logger.LogWarning("AnalyseParkinson — no ACC records found for device {D}, date {Date}", deviceId, date);
            return null;
        }

        // Flatten X/Y/Z values in time order
        var xList = new List<int>();
        var yList = new List<int>();
        var zList = new List<int>();

        foreach (var r in records.OrderBy(r => r.DataTime))
        {
            xList.AddRange(JsonSerializer.Deserialize<List<int>>(r.XValuesJson) ?? []);
            yList.AddRange(JsonSerializer.Deserialize<List<int>>(r.YValuesJson) ?? []);
            zList.AddRange(JsonSerializer.Deserialize<List<int>>(r.ZValuesJson) ?? []);
        }

        _logger.LogInformation("AnalyseParkinson — calling algo, device {D}, ACC samples {N}", deviceId, xList.Count);

        var result = await _algoClient.CalculateParkinsonAsync(
            new ParkinsonCalcRequest(xList, yList, zList, deviceId), ct).ConfigureAwait(false);

        if (result is null) return null;

        return new ParkinsonAnalysisDto
        {
            DeviceId = deviceId,
            Date = date,
            TremorScore = result.TremorScore,
            ActivityScore = result.ActivityScore
        };
    }
}
