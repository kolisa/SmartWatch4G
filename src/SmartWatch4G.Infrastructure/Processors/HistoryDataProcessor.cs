using Google.Protobuf;
using Microsoft.Extensions.Logging;
using SmartWatch4G.Application.Utilities;

namespace SmartWatch4G.Infrastructure.Processors;

public class HistoryDataProcessor
{
    private readonly ILogger<HistoryDataProcessor> _logger;

    public HistoryDataProcessor(ILogger<HistoryDataProcessor> logger)
    {
        _logger = logger;
    }

    public void ProceedHistoryData(byte[] pbData)
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

        if (hisNotify.DataCase == HisNotification.DataOneofCase.IndexTable)
        {
            if (hisNotify.Type == HisDataType.YylpfeData)
            {
                foreach (var index in hisNotify.IndexTable.Index)
                    _logger.LogInformation("startSeq: {Start}, endSeq: {End}, time: {Time}",
                        index.StartSeq, index.EndSeq, index.Time);
            }
            return;
        }

        if (hisNotify.DataCase != HisNotification.DataOneofCase.HisData) return;

        var hisData = hisNotify.HisData;
        _logger.LogInformation("seq: {Seq}", hisData.Seq);

        switch (hisNotify.Type)
        {
            case HisDataType.HealthData:
                if (hisData.Health != null) ParseHealthData(hisData.Health);
                break;
            case HisDataType.EcgData:
                if (hisData.Ecg != null) ParseEcgData(hisData.Ecg);
                break;
            case HisDataType.RriData:
                if (hisData.Rri != null) ParseRriData(hisData.Rri);
                break;
            case HisDataType.Spo2Data:
                if (hisData.Spo2 != null) ParseSpo2Data(hisData.Spo2);
                break;
            case HisDataType.ThirdpartyData:
                if (hisData.ThirdPartyData != null) ParseThirdPartyData(hisData.ThirdPartyData);
                break;
            case HisDataType.PpgData:
                if (hisData.Ppg != null) ParsePpgData(hisData.Ppg);
                break;
            case HisDataType.AccelerometerData:
                if (hisData.ACCelerometerData != null) ParseAccData(hisData.ACCelerometerData);
                break;
            case HisDataType.MultiLeadsEcgData:
                if (hisData.MultiLeadsECG != null) ParseMultiLeadsEcgData(hisData.MultiLeadsECG);
                break;
        }
    }

    private void ParseHealthData(HisDataHealth hisHealthData)
    {
        string timeStr = DateTimeUtilities.FromUnixSeconds(hisHealthData.TimeStamp.DateTime_.Seconds);

        if (hisHealthData.PedoData != null)
        {
            float distance = hisHealthData.PedoData.Distance * 0.1f;
            float calorie  = hisHealthData.PedoData.Calorie  * 0.1f;
            _logger.LogInformation("----{Time} step:{Step}, distance:{Dist}, calorie:{Cal}",
                timeStr, hisHealthData.PedoData.Step, distance, calorie);
        }

        if (hisHealthData.HrData != null)
            _logger.LogInformation("----{Time} avg hr:{Avg}, max hr:{Max}, min hr:{Min}",
                timeStr, hisHealthData.HrData.AvgBpm, hisHealthData.HrData.MaxBpm, hisHealthData.HrData.MinBpm);

        if (hisHealthData.BxoyData != null)
            _logger.LogInformation("----{Time} avg boxy:{Avg}, max boxy:{Max}, min boxy:{Min}",
                timeStr, hisHealthData.BxoyData.AgvOxy, hisHealthData.BxoyData.MaxOxy, hisHealthData.BxoyData.MinOxy);

        if (hisHealthData.BpData != null)
            _logger.LogInformation("----{Time} sbp:{Sbp}, dbp:{Dbp}",
                timeStr, hisHealthData.BpData.Sbp, hisHealthData.BpData.Dbp);

        if (hisHealthData.HrvData != null)
        {
            int fatigue = (int)hisHealthData.HrvData.Fatigue;
            if (fatigue <= 0) fatigue = (int)(Math.Log(hisHealthData.HrvData.RMSSD) * 20);
            _logger.LogInformation("----{Time} fatigue:{Fatigue}", timeStr, fatigue);
        }
    }

    private void ParseEcgData(HisDataECG d)
    {
        string timeStr = DateTimeUtilities.FromUnixSeconds(d.TimeStamp.DateTime_.Seconds);
        _logger.LogInformation("----{Time} ecg count:{Count}", timeStr, d.RawData.Count);
    }

    private void ParseRriData(HisDataRRI d)
    {
        string timeStr = DateTimeUtilities.FromUnixSeconds(d.TimeStamp.DateTime_.Seconds);
        var rriList = new List<short>(d.RawData.Count * 2);
        foreach (var raw in d.RawData)
        {
            long v = (long)raw;
            rriList.Add((short)((v >> 16) & 0xffff));
            rriList.Add((short)(v & 0xffff));
        }
        _logger.LogInformation("----{Time} rri count:{Count}", timeStr, rriList.Count);
    }

    private void ParseSpo2Data(HisDataSpo2 d)
    {
        string timeStr = DateTimeUtilities.FromUnixSeconds(d.TimeStamp.DateTime_.Seconds);
        foreach (var raw in d.Spo2Data)
        {
            int spo2 = (int)((raw >> 24) & 0xFF);
            int hr   = (int)((raw >> 16) & 0xFF);
            _logger.LogInformation("----{Time} spo2:{Spo2}, hr:{Hr}", timeStr, spo2, hr);
        }
    }

    private void ParseThirdPartyData(HisDataThirdParty d)
    {
        if (d.DataHealth?.MacAddr != null)
            _logger.LogInformation("----3rd party device mac:{Mac}", d.DataHealth.MacAddr);
    }

    private void ParsePpgData(HisDataPPG d)
    {
        string timeStr = DateTimeUtilities.FromUnixSeconds(d.TimeStamp.DateTime_.Seconds);
        _logger.LogInformation("----{Time} ppg count:{Count}", timeStr, d.RawData.Count);
    }

    private void ParseAccData(HisACCelerometer d)
    {
        string timeStr = DateTimeUtilities.FromUnixSeconds(d.TimeStamp.DateTime_.Seconds);
        _logger.LogInformation("----{Time} acc data", timeStr);
    }

    private void ParseMultiLeadsEcgData(HisDataMultiLeadsECG d)
    {
        string timeStr = DateTimeUtilities.FromUnixSeconds(d.TimeStamp.DateTime_.Seconds);
        _logger.LogInformation("----{Time} multi-leads ecg channels:{Ch}", timeStr, d.NumberOfChannels);
    }
}
