using System.Text.Json;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using Google.Protobuf;
using Microsoft.Extensions.Logging;

namespace SmartWatch4G.Infrastructure.Processors;

/// <summary>
/// Preprocesses health data from opcode 0x80 packets to build per-slot sleep
/// JSON strings.  Each persisted row represents one five-minute slot; a
/// downstream sleep-stage engine combines all slots for a given day.
/// The combined string is passed to the iwown algo service
/// (<c>POST /calculation/sleep</c>) via <see cref="SleepQueryService"/>.
/// Replaces the original <c>SleepPreprocessor</c> flat-class.
/// </summary>
public sealed class SleepPreprocessor
{
    private readonly ISleepDataRepository _sleepRepo;
    private readonly ILogger<SleepPreprocessor> _logger;

    public SleepPreprocessor(ISleepDataRepository sleepRepo, ILogger<SleepPreprocessor> logger)
    {
        _sleepRepo = sleepRepo;
        _logger = logger;
    }

    public async Task PrepareSleepDataAsync(
        string deviceId,
        byte[] pbData,
        CancellationToken cancellationToken = default)
    {
        HisNotification hisNotify;
        try
        {
            hisNotify = HisNotification.Parser.ParseFrom(pbData);
        }
        catch (InvalidProtocolBufferException ex)
        {
            _logger.LogError("Sleep preprocessor — parse error: {Message}", ex.Message);
            return;
        }

        if (hisNotify.DataCase != HisNotification.DataOneofCase.HisData ||
            hisNotify.Type != HisDataType.HealthData ||
            hisNotify.HisData.Health is null)
        {
            return;
        }

        var hisHealth = hisNotify.HisData.Health;
        string dataTime = DateTimeUtilities.FromUnixSeconds(hisHealth.TimeStamp.DateTime_.Seconds);

        // Build the compact slot dictionary used by the sleep algorithm
        var slotDict = new Dictionary<string, object>
        {
            ["Q"] = hisNotify.HisData.Seq
        };

        // Parse hour/minute from the slot timestamp
        if (DateTime.TryParseExact(
            dataTime,
            "yyyy-MM-dd HH:mm:ss",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out DateTime dt))
        {
            slotDict["T"] = new int[] { dt.Hour, dt.Minute };
        }

        // Steps
        if (hisHealth.PedoData is not null)
        {
            slotDict["P"] = new Dictionary<string, object>
            {
                ["s"] = hisHealth.PedoData.Step,
                ["c"] = hisHealth.PedoData.Calorie,
                ["d"] = hisHealth.PedoData.Distance,
                ["t"] = hisHealth.PedoData.Type,
                ["a"] = hisHealth.PedoData.State & 15u
            };
        }

        // HRV / stress
        if (hisHealth.HrvData is not null)
        {
            var hrv = hisHealth.HrvData;
            var hrvDict = new Dictionary<string, object>();
            if (hrv.SDNN > 0) hrvDict["s"] = hrv.SDNN / 10.0;
            if (hrv.RMSSD > 0) hrvDict["r"] = hrv.RMSSD / 10.0;
            if (hrv.PNN50 > 0) hrvDict["p"] = hrv.PNN50 / 10.0;
            if (hrv.MEAN > 0) hrvDict["m"] = hrv.MEAN / 10.0;

            int fatigue = (int)hrv.Fatigue;
            if (fatigue > -1000)
            {
                if (fatigue <= 0) fatigue = (int)(Math.Log((double)hrv.RMSSD) * 20);
                if (fatigue > 0) hrvDict["f"] = fatigue;
                if (hrvDict.Count > 0) slotDict["V"] = hrvDict;
            }
        }

        // Sleep state byte sequence
        if (hisHealth.SleepData is not null)
        {
            var sleepDict = new Dictionary<string, object>
            {
                ["a"] = hisHealth.SleepData.SleepData
            };
            if (hisHealth.SleepData.ShutDown) sleepDict["s"] = hisHealth.SleepData.ShutDown;
            if (hisHealth.SleepData.Charge) sleepDict["c"] = hisHealth.SleepData.Charge;
            slotDict["E"] = sleepDict;
        }

        string sleepJson;
        try
        {
            sleepJson = JsonSerializer.Serialize(slotDict);
        }
        catch (JsonException ex)
        {
            _logger.LogError("Sleep JSON serialisation failed: {Message}", ex.Message);
            return;
        }

        _logger.LogInformation("{Time} seq:{Seq} — sleep slot: {Json}", dataTime, hisNotify.HisData.Seq, sleepJson);

        // Sleep date is determined by the timestamp's date component
        string sleepDate = dataTime.Length >= 10 ? dataTime[..10] : dataTime;

        await _sleepRepo.AddAsync(new SleepDataRecord
        {
            DeviceId = deviceId,
            SleepDate = sleepDate,
            DataTime = dataTime,
            Seq = hisNotify.HisData.Seq,
            SleepJson = sleepJson
        }, cancellationToken).ConfigureAwait(false);
    }
}
