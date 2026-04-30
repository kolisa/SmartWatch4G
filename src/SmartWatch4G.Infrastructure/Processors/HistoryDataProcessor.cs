using Google.Protobuf;
using Microsoft.Extensions.Logging;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Interfaces;

namespace SmartWatch4G.Infrastructure.Processors;

public class HistoryDataProcessor
{
    private readonly ILogger<HistoryDataProcessor> _logger;
    private readonly IDatabaseService _db;

    public HistoryDataProcessor(ILogger<HistoryDataProcessor> logger, IDatabaseService db)
    {
        _logger = logger;
        _db     = db;
    }

    public async Task ProceedHistoryData(byte[] pbData, string deviceId = "")
    {
        HisNotification hisNotify;
        try
        {
            hisNotify = HisNotification.Parser.ParseFrom(pbData);
        }
        catch (InvalidProtocolBufferException e)
        {
            _logger.LogError(e, "Parse 80 health history error: {Message}", e.Message);
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
                if (hisData.Health != null) await ParseHealthData(hisData.Health, deviceId);
                break;
            case HisDataType.EcgData:
                if (hisData.Ecg != null) await ParseEcgData(hisData.Ecg, deviceId);
                break;
            case HisDataType.RriData:
                if (hisData.Rri != null) await ParseRriData(hisData.Rri, deviceId);
                break;
            case HisDataType.Spo2Data:
                if (hisData.Spo2 != null) await ParseSpo2Data(hisData.Spo2, deviceId);
                break;
            case HisDataType.ThirdpartyData:
                if (hisData.ThirdPartyData != null) await ParseThirdPartyData(hisData.ThirdPartyData, deviceId);
                break;
            case HisDataType.PpgData:
                if (hisData.Ppg != null) await ParsePpgData(hisData.Ppg, deviceId);
                break;
            case HisDataType.AccelerometerData:
                if (hisData.ACCelerometerData != null) await ParseAccData(hisData.ACCelerometerData, deviceId);
                break;
            case HisDataType.MultiLeadsEcgData:
                if (hisData.MultiLeadsECG != null) await ParseMultiLeadsEcgData(hisData.MultiLeadsECG, deviceId);
                break;
        }
    }

    private async Task ParseHealthData(HisDataHealth d, string deviceId)
    {
        string timeStr = DateTimeUtilities.FromUnixSeconds(d.TimeStamp.DateTime_.Seconds);

        int?    steps    = null;
        double? distance = null;
        double? calorie  = null;
        if (d.PedoData != null)
        {
            steps    = (int?)d.PedoData.Step;
            distance = d.PedoData.Distance * 0.1;
            calorie  = d.PedoData.Calorie  * 0.1;
            _logger.LogInformation("----{Time} step:{Step}, distance:{Dist}, calorie:{Cal}",
                timeStr, steps, distance, calorie);
        }

        int? avgHr = null, maxHr = null, minHr = null;
        if (d.HrData != null)
        {
            avgHr = (int?)d.HrData.AvgBpm;
            maxHr = (int?)d.HrData.MaxBpm;
            minHr = (int?)d.HrData.MinBpm;
            _logger.LogInformation("----{Time} avg hr:{Avg}, max hr:{Max}, min hr:{Min}",
                timeStr, avgHr, maxHr, minHr);
        }

        int? avgSpo2 = null;
        if (d.BxoyData != null)
        {
            avgSpo2 = (int?)d.BxoyData.AgvOxy;
            _logger.LogInformation("----{Time} avg boxy:{Avg}, max boxy:{Max}, min boxy:{Min}",
                timeStr, d.BxoyData.AgvOxy, d.BxoyData.MaxOxy, d.BxoyData.MinOxy);
        }

        int? sbp = null, dbp = null;
        if (d.BpData != null)
        {
            sbp = (int?)d.BpData.Sbp;
            dbp = (int?)d.BpData.Dbp;
            _logger.LogInformation("----{Time} sbp:{Sbp}, dbp:{Dbp}", timeStr, sbp, dbp);
        }

        int? fatigue = null;
        if (d.HrvData != null)
        {
            fatigue = (int)d.HrvData.Fatigue;
            if (fatigue <= 0) fatigue = (int)(Math.Log(d.HrvData.RMSSD) * 20);
            _logger.LogInformation("----{Time} fatigue:{Fatigue}", timeStr, fatigue);
        }

        double? bodyTempEvi = null;
        int?    bodyTempEsti = null, tempType = null;
        if (d.TemperatureData != null)
        {
            bodyTempEvi  = d.TemperatureData.HasEviBody  ? d.TemperatureData.EviBody  * 0.01 : null;
            bodyTempEsti = d.TemperatureData.HasEstiArm  ? (int?)d.TemperatureData.EstiArm   : null;
            tempType     = d.TemperatureData.HasType     ? (int?)d.TemperatureData.Type       : null;
            _logger.LogInformation("----{Time} temp evi:{Evi}, esti:{Esti}", timeStr, bodyTempEvi, bodyTempEsti);
        }

        double? biozR = null, biozX = null, biozFat = null, biozBmi = null;
        int?    biozType = null;
        if (d.BiozData != null)
        {
            biozR    = d.BiozData.HasR    ? (double?)d.BiozData.R              : null;
            biozX    = d.BiozData.HasX    ? (double?)d.BiozData.X              : null;
            biozFat  = d.BiozData.HasFat  ? d.BiozData.Fat  * 0.1             : null;
            biozBmi  = d.BiozData.HasBmi  ? d.BiozData.Bmi  * 0.1             : null;
            biozType = d.BiozData.HasType ? (int?)d.BiozData.Type              : null;
            _logger.LogInformation("----{Time} bioz R:{R}, X:{X}, fat:{Fat}, bmi:{Bmi}", timeStr, biozR, biozX, biozFat, biozBmi);
        }

        double? bloodSugar = null;
        if (d.BloodSugarData?.HasBloodSugar == true)
            bloodSugar = d.BloodSugarData.BloodSugar * 0.01;

        int? bpBpm = null;
        if (d.BpBpmData?.HasBpm == true)
            bpBpm = (int?)d.BpBpmData.Bpm;

        double? bloodPotassium = null;
        if (d.BloodPotassiumData?.HasBloodPotassium == true)
            bloodPotassium = d.BloodPotassiumData.BloodPotassium * 0.01;

        double? breathRate = null;
        if (d.BreathData?.HasBreathrate == true)
            breathRate = d.BreathData.Breathrate;

        int? moodLevel = null;
        if (d.MoodData?.HasMoodLevel == true)
            moodLevel = (int?)d.MoodData.MoodLevel;

        if (!string.IsNullOrEmpty(deviceId))
            await _db.UpsertHealthSnapshot(deviceId, timeStr,
                steps: steps, distance: distance, calorie: calorie,
                avgHr: avgHr, maxHr: maxHr, minHr: minHr,
                avgSpo2: avgSpo2, sbp: sbp, dbp: dbp, fatigue: fatigue,
                bodyTempEvi: bodyTempEvi, bodyTempEsti: bodyTempEsti, tempType: tempType,
                bpBpm: bpBpm, bloodPotassium: bloodPotassium, bloodSugar: bloodSugar,
                biozR: biozR, biozX: biozX, biozFat: biozFat, biozBmi: biozBmi, biozType: biozType,
                breathRate: breathRate, moodLevel: moodLevel);
    }

    private async Task ParseEcgData(HisDataECG d, string deviceId)
    {
        string timeStr = DateTimeUtilities.FromUnixSeconds(d.TimeStamp.DateTime_.Seconds);
        _logger.LogInformation("----{Time} ecg count:{Count}", timeStr, d.RawData.Count);
        if (string.IsNullOrEmpty(deviceId) || d.RawData.Count == 0) return;
        var json = System.Text.Json.JsonSerializer.Serialize(d.RawData);
        await _db.InsertEcgWaveform(deviceId, timeStr, d.RawData.Count, json);
    }

    private async Task ParseRriData(HisDataRRI d, string deviceId)
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
        if (string.IsNullOrEmpty(deviceId) || rriList.Count == 0) return;
        var json = System.Text.Json.JsonSerializer.Serialize(rriList);
        await _db.InsertRriWaveform(deviceId, timeStr, rriList.Count, json);
    }

    private async Task ParseSpo2Data(HisDataSpo2 d, string deviceId)
    {
        string timeStr = DateTimeUtilities.FromUnixSeconds(d.TimeStamp.DateTime_.Seconds);
        var readings = new List<object>(d.Spo2Data.Count);
        foreach (var raw in d.Spo2Data)
        {
            int spo2 = (int)((raw >> 24) & 0xFF);
            int hr   = (int)((raw >> 16) & 0xFF);
            _logger.LogInformation("----{Time} spo2:{Spo2}, hr:{Hr}", timeStr, spo2, hr);
            readings.Add(new { spo2, hr });
        }
        if (string.IsNullOrEmpty(deviceId) || readings.Count == 0) return;
        var json = System.Text.Json.JsonSerializer.Serialize(readings);
        await _db.InsertSpo2Waveform(deviceId, timeStr, json);
    }

    private async Task ParseThirdPartyData(HisDataThirdParty d, string deviceId)
    {
        if (!d.HasDataHealth) return;
        var h = d.DataHealth;
        string mac     = h.MacAddr ?? string.Empty;
        string devName = h.DevName ?? string.Empty;

        if (h.HasBpData)
            await _db.InsertThirdPartyReading(deviceId, mac, devName, "bp", null,
                sbp: h.BpData.HasSbp   ? (double?)h.BpData.Sbp   : null,
                dbp: h.BpData.HasDbp   ? (double?)h.BpData.Dbp   : null,
                hr:  h.BpData.HasHr    ? (double?)h.BpData.Hr    : null,
                pulse: h.BpData.HasPulse ? (double?)h.BpData.Pulse : null,
                weight: null, impedance: null, bodyFatPct: null,
                spo2: null, pi: null, bodyTemp: null, value: null);

        if (h.HasScaleData)
            await _db.InsertThirdPartyReading(deviceId, mac, devName, "scale", null,
                sbp: null, dbp: null, hr: null, pulse: null,
                weight:     h.ScaleData.HasWeight            ? (double?)h.ScaleData.Weight            : null,
                impedance:  h.ScaleData.HasImpedance         ? (double?)h.ScaleData.Impedance         : null,
                bodyFatPct: h.ScaleData.HasBodyFatPercentage ? h.ScaleData.BodyFatPercentage * 0.1    : null,
                spo2: null, pi: null, bodyTemp: null, value: null);

        if (h.HasSpo2Data)
            await _db.InsertThirdPartyReading(deviceId, mac, devName, "spo2", null,
                sbp: null, dbp: null,
                hr:    h.Spo2Data.HasBpm  ? (double?)h.Spo2Data.Bpm  : null,
                pulse: null,
                weight: null, impedance: null, bodyFatPct: null,
                spo2: h.Spo2Data.HasSpo2 ? (double?)h.Spo2Data.Spo2 : null,
                pi:   h.Spo2Data.HasPi   ? h.Spo2Data.Pi * 0.1      : null,
                bodyTemp: null, value: null);

        if (h.HasTempData)
            await _db.InsertThirdPartyReading(deviceId, mac, devName, "temperature", null,
                sbp: null, dbp: null, hr: null, pulse: null,
                weight: null, impedance: null, bodyFatPct: null, spo2: null, pi: null,
                bodyTemp: h.TempData.HasBodyTemp ? h.TempData.BodyTemp * 0.01 : null,
                value: null);

        if (h.HasGluData)
            await _db.InsertThirdPartyReading(deviceId, mac, devName, "glucose", null,
                sbp: null, dbp: null, hr: null, pulse: null,
                weight: null, impedance: null, bodyFatPct: null, spo2: null, pi: null,
                bodyTemp: null,
                value: h.GluData.HasGlu ? h.GluData.Glu * 0.01 : null);

        if (h.HasBloodKetonesData)
            await _db.InsertThirdPartyReading(deviceId, mac, devName, "blood_ketones", null,
                sbp: null, dbp: null, hr: null, pulse: null,
                weight: null, impedance: null, bodyFatPct: null, spo2: null, pi: null,
                bodyTemp: null,
                value: h.BloodKetonesData.HasBloodKetones ? h.BloodKetonesData.BloodKetones * 0.01 : null);

        if (h.HasUricAcidData)
            await _db.InsertThirdPartyReading(deviceId, mac, devName, "uric_acid", null,
                sbp: null, dbp: null, hr: null, pulse: null,
                weight: null, impedance: null, bodyFatPct: null, spo2: null, pi: null,
                bodyTemp: null,
                value: h.UricAcidData.HasUricAcid ? (double?)h.UricAcidData.UricAcid : null);

        _logger.LogInformation("----3rd party mac:{Mac} dev:{Dev}", mac, devName);
    }

    private async Task ParsePpgData(HisDataPPG d, string deviceId)
    {
        string timeStr = DateTimeUtilities.FromUnixSeconds(d.TimeStamp.DateTime_.Seconds);
        _logger.LogInformation("----{Time} ppg count:{Count}", timeStr, d.RawData.Count);
        if (string.IsNullOrEmpty(deviceId) || d.RawData.Count == 0) return;
        var json = System.Text.Json.JsonSerializer.Serialize(d.RawData);
        await _db.InsertPpgWaveform(deviceId, timeStr, d.RawData.Count, json);
    }

    private async Task ParseAccData(HisACCelerometer d, string deviceId)
    {
        string timeStr = DateTimeUtilities.FromUnixSeconds(d.TimeStamp.DateTime_.Seconds);
        _logger.LogInformation("----{Time} acc data count:{Count}", timeStr, d.AccDataCount);
        if (string.IsNullOrEmpty(deviceId)) return;
        var accX = d.HasAccX ? Convert.ToBase64String(d.AccX.ToByteArray()) : null;
        var accY = d.HasAccY ? Convert.ToBase64String(d.AccY.ToByteArray()) : null;
        var accZ = d.HasAccZ ? Convert.ToBase64String(d.AccZ.ToByteArray()) : null;
        await _db.InsertAccWaveform(deviceId, timeStr, (int)d.AccDataCount, accX, accY, accZ);
    }

    private async Task ParseMultiLeadsEcgData(HisDataMultiLeadsECG d, string deviceId)
    {
        string timeStr = DateTimeUtilities.FromUnixSeconds(d.TimeStamp.DateTime_.Seconds);
        _logger.LogInformation("----{Time} multi-leads ecg channels:{Ch}", timeStr, d.NumberOfChannels);
        if (string.IsNullOrEmpty(deviceId) || d.RawData == ByteString.Empty) return;
        var rawBase64 = Convert.ToBase64String(d.RawData.ToByteArray());
        await _db.InsertMultiLeadsEcgWaveform(deviceId, timeStr,
            (int)d.NumberOfChannels, (int)d.SingleDataByteLen, rawBase64);
    }
}
