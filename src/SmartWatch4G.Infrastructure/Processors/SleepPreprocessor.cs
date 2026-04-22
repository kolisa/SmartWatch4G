using Google.Protobuf;
using Microsoft.Extensions.Logging;
using SmartWatch4G.Application.Utilities;

namespace SmartWatch4G.Infrastructure.Processors;

public class SleepPreprocessor
{
    private readonly ILogger<SleepPreprocessor> _logger;

    public SleepPreprocessor(ILogger<SleepPreprocessor> logger)
    {
        _logger = logger;
    }

    public void PrepareSleepData(byte[] pbData)
    {
        HisNotification hisNotify;
        try
        {
            hisNotify = HisNotification.Parser.ParseFrom(pbData);
        }
        catch (InvalidProtocolBufferException e)
        {
            _logger.LogError("Parse 80 health history error: {Message}", e.Message);
            return;
        }

        if (hisNotify.DataCase != HisNotification.DataOneofCase.HisData) return;

        var hisData = hisNotify.HisData;
        _logger.LogInformation("seq: {Seq}", hisData.Seq);

        if (hisNotify.Type != HisDataType.HealthData || hisData.Health == null) return;

        string timeStr = DateTimeUtilities.FromUnixSeconds(hisData.Health.TimeStamp.DateTime_.Seconds);

        var sleepDict = new Dictionary<string, object> { ["Q"] = hisData.Seq };
        var dt = System.DateTime.ParseExact(timeStr, "yyyy-MM-dd HH:mm:ss",
            System.Globalization.CultureInfo.InvariantCulture);
        sleepDict["T"] = new[] { dt.Hour, dt.Minute };

        if (hisData.Health.PedoData != null)
            sleepDict["P"] = new Dictionary<string, object>
            {
                ["s"] = hisData.Health.PedoData.Step,
                ["c"] = hisData.Health.PedoData.Calorie,
                ["d"] = hisData.Health.PedoData.Distance,
                ["t"] = hisData.Health.PedoData.Type,
                ["a"] = hisData.Health.PedoData.State & 15
            };

        if (hisData.Health.HrvData is { } hrv)
        {
            int fatigue = (int)hrv.Fatigue;
            if (fatigue <= 0) fatigue = (int)(Math.Log((double)hrv.RMSSD) * 20);
            var d = new Dictionary<string, object>();
            if (hrv.SDNN  > 0) d["s"] = hrv.SDNN  / 10.0;
            if (hrv.RMSSD > 0) d["r"] = hrv.RMSSD / 10.0;
            if (hrv.PNN50 > 0) d["p"] = hrv.PNN50 / 10.0;
            if (hrv.MEAN  > 0) d["m"] = hrv.MEAN  / 10.0;
            if (fatigue > 0) d["f"] = fatigue;
            if (d.Count > 0) sleepDict["V"] = d;
        }

        if (hisData.Health.SleepData != null)
            sleepDict["E"] = new Dictionary<string, object>
            {
                ["a"] = hisData.Health.SleepData.SleepData,
                ["s"] = hisData.Health.SleepData.ShutDown,
                ["c"] = hisData.Health.SleepData.Charge
            };

        try
        {
            string sleepStr = System.Text.Json.JsonSerializer.Serialize(sleepDict);
            _logger.LogInformation("{Time} {Seq} {SleepStr}", timeStr, hisData.Seq, sleepStr);
        }
        catch (Exception ex) { _logger.LogError(ex.Message); }
    }
}
