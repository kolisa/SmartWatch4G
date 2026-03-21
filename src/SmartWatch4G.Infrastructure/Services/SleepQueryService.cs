using System.Text.Json;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using SmartWatch4G.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace SmartWatch4G.Infrastructure.Services;

/// <summary>
/// Retrieves computed sleep results for a device and date by:
/// 1. Loading pre-processed sleep slots from the database for recordDate and recordDate-1.
/// 2. Combining those slots into the JSON-array strings the algo API expects.
/// 3. Loading RRI data for both days and flattening into lists.
/// 4. Calling the iwown algo service (POST /calculation/sleep).
/// 5. Mapping the sleep sections (type 3/4/6/7) to the minute-count fields
///    the device expects in the GET /health/sleep response.
/// </summary>
public sealed class SleepQueryService : ISleepQueryService
{
    private const int TypeDeepSleep = 3;
    private const int TypeLightSleep = 4;
    private const int TypeAwake = 6;
    private const int TypeRem = 7;

    private readonly ISleepDataRepository _sleepRepo;
    private readonly IRriDataRepository _rriRepo;
    private readonly IWownAlgoClient _algoClient;
    private readonly ILogger<SleepQueryService> _logger;

    public SleepQueryService(
        ISleepDataRepository sleepRepo,
        IRriDataRepository rriRepo,
        IWownAlgoClient algoClient,
        ILogger<SleepQueryService> logger)
    {
        _sleepRepo = sleepRepo;
        _rriRepo = rriRepo;
        _algoClient = algoClient;
        _logger = logger;
    }

    public async Task<SleepResult?> GetSleepResultAsync(
        string deviceId,
        string sleepDate,
        CancellationToken cancellationToken = default)
    {
        string prevDate = DateTimeUtilities.GetPreviousDay(sleepDate);
        if (string.IsNullOrEmpty(prevDate))
        {
            _logger.LogWarning("Invalid sleepDate: {Date}", sleepDate);
            return null;
        }

        IReadOnlyList<SleepDataRecord> prevSlots =
            await _sleepRepo.GetByDeviceAndDateAsync(deviceId, prevDate, cancellationToken)
                            .ConfigureAwait(false);
        IReadOnlyList<SleepDataRecord> nextSlots =
            await _sleepRepo.GetByDeviceAndDateAsync(deviceId, sleepDate, cancellationToken)
                            .ConfigureAwait(false);

        if (prevSlots.Count == 0 && nextSlots.Count == 0)
        {
            _logger.LogInformation("No sleep data for device {DeviceId} on {Date}", deviceId, sleepDate);
            return null;
        }

        string prevDaySleepJson = CombineSleepSlots(prevSlots);
        string nextDaySleepJson = CombineSleepSlots(nextSlots);

        IReadOnlyList<long> prevDayRri =
            await FlattenRriAsync(deviceId, prevDate, cancellationToken).ConfigureAwait(false);
        IReadOnlyList<long> nextDayRri =
            await FlattenRriAsync(deviceId, sleepDate, cancellationToken).ConfigureAwait(false);

        if (!int.TryParse(sleepDate.Replace("-", ""), out int recordDate))
        {
            _logger.LogWarning("Cannot parse sleepDate {Date} as int", sleepDate);
            return null;
        }

        var calcRequest = new SleepCalcRequest(
            PrevDaySleepJson: prevDaySleepJson,
            NextDaySleepJson: nextDaySleepJson,
            PrevDayRriList: prevDayRri,
            NextDayRriList: nextDayRri,
            RecordDate: recordDate,
            DeviceId: deviceId);

        SleepCalcResult? calc = await _algoClient
            .CalculateSleepAsync(calcRequest, cancellationToken)
            .ConfigureAwait(false);

        if (calc is null)
        {
            return null;
        }

        return new SleepResult(
            DeviceId: deviceId,
            SleepDate: sleepDate,
            StartTime: calc.StartTime,
            EndTime: calc.EndTime,
            DeepSleepMinutes: SumMinutes(calc.Sections, TypeDeepSleep),
            LightSleepMinutes: SumMinutes(calc.Sections, TypeLightSleep),
            WeakSleepMinutes: SumMinutes(calc.Sections, TypeAwake),
            EyeMoveSleepMinutes: SumMinutes(calc.Sections, TypeRem),
            Score: 0,
            OsahsRisk: 0,
            Spo2Score: 0,
            SleepHeartRate: calc.HeartRate);
    }

    private static string CombineSleepSlots(IReadOnlyList<SleepDataRecord> slots)
    {
        if (slots.Count == 0) return "[]";
        return "[" + string.Join(",", slots.Select(s => s.SleepJson)) + "]";
    }

    private async Task<IReadOnlyList<long>> FlattenRriAsync(
        string deviceId,
        string date,
        CancellationToken ct)
    {
        IReadOnlyList<RriDataRecord> records =
            await _rriRepo.GetByDeviceAndDateAsync(deviceId, date, ct).ConfigureAwait(false);

        if (records.Count == 0) return [];

        var flat = new List<long>();
        foreach (var rec in records)
        {
            try
            {
                var values = JsonSerializer.Deserialize<List<long>>(rec.RriValuesJson);
                if (values is not null) flat.AddRange(values);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning("Failed to parse RRI JSON for record {Id}: {Msg}", rec.Id, ex.Message);
            }
        }

        return flat;
    }

    private static int SumMinutes(IReadOnlyList<SleepSection> sections, int type)
    {
        int total = 0;
        foreach (var section in sections)
        {
            if (section.Type != type) continue;
            if (System.DateTime.TryParse(section.Start, out System.DateTime start) &&
                System.DateTime.TryParse(section.End, out System.DateTime end))
            {
                total += (int)(end - start).TotalMinutes;
            }
        }
        return total;
    }
}
