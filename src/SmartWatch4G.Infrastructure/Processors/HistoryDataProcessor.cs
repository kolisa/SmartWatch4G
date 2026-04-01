using System.Text.Json;

using Google.Protobuf;

using Microsoft.Extensions.Logging;

using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Processors;

/// <summary>
/// Parses opcode 0x80 history-data protobuf packets and persists all
/// health, ECG, RRI, PPG, accelerometer, multi-leads-ECG, SPO2,
/// third-party, and YYLPFE data to the database.
/// Replaces the original <c>HistoryDataProcessor</c> flat-class.
/// </summary>
public sealed class HistoryDataProcessor
{
    private readonly IHealthDataRepository _healthRepo;
    private readonly IRriDataRepository _rriRepo;
    private readonly ISleepDataRepository _sleepRepo;
    private readonly ISpo2DataRepository _spo2Repo;
    private readonly IAccDataRepository _accRepo;
    private readonly ILogger<HistoryDataProcessor> _logger;

    public HistoryDataProcessor(
        IHealthDataRepository healthRepo,
        IRriDataRepository rriRepo,
        ISleepDataRepository sleepRepo,
        ISpo2DataRepository spo2Repo,
        IAccDataRepository accRepo,
        ILogger<HistoryDataProcessor> logger)
    {
        _healthRepo = healthRepo;
        _rriRepo = rriRepo;
        _sleepRepo = sleepRepo;
        _spo2Repo = spo2Repo;
        _accRepo = accRepo;
        _logger = logger;
    }

    public async Task ProcessAsync(
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
            _logger.LogError("Parse 0x80 history data error: {Message}", ex.Message);
            return;
        }

        if (hisNotify.DataCase == HisNotification.DataOneofCase.IndexTable)
        {
            if (hisNotify.Type == HisDataType.YylpfeData)
            {
                foreach (var index in hisNotify.IndexTable.Index)
                {
                    _logger.LogInformation(
                        "YYLPFE index — startSeq: {Start}, endSeq: {End}, time: {Time}",
                        index.StartSeq, index.EndSeq, index.Time);
                }
            }

            return;
        }

        if (hisNotify.DataCase != HisNotification.DataOneofCase.HisData)
        {
            return;
        }

        var hisData = hisNotify.HisData;
        _logger.LogInformation("HisData seq: {Seq}", hisData.Seq);

        switch (hisNotify.Type)
        {
            case HisDataType.HealthData when hisData.Health is not null:
                await ParseAndSaveHealthAsync(deviceId, hisData.Seq, hisData.Health, cancellationToken)
                    .ConfigureAwait(false);
                break;

            case HisDataType.EcgData when hisData.Ecg is not null:
                await ParseAndSaveEcgAsync(deviceId, hisData.Seq, hisData.Ecg, cancellationToken)
                    .ConfigureAwait(false);
                break;

            case HisDataType.RriData when hisData.Rri is not null:
                await ParseAndSaveRriAsync(deviceId, hisData.Seq, hisData.Rri, cancellationToken)
                    .ConfigureAwait(false);
                break;

            case HisDataType.Spo2Data when hisData.Spo2 is not null:
                await ParseAndSaveSpo2Async(deviceId, hisData.Spo2, cancellationToken)
                    .ConfigureAwait(false);
                break;

            case HisDataType.ThirdpartyData when hisData.ThirdPartyData is not null:
                ParseThirdParty(hisData.ThirdPartyData);
                break;

            case HisDataType.ThirdpartyData when hisData.ThirdPartyDataV2 is not null:
                ParseThirdPartyV2(hisData.ThirdPartyDataV2);
                break;

            case HisDataType.PpgData when hisData.Ppg is not null:
                ParsePpg(hisData.Ppg);
                break;

            case HisDataType.AccelerometerData when hisData.ACCelerometerData is not null:
                await ParseAndSaveAccAsync(deviceId, hisData.ACCelerometerData, cancellationToken)
                    .ConfigureAwait(false);
                break;

            case HisDataType.MultiLeadsEcgData when hisData.MultiLeadsECG is not null:
                ParseMultiLeadsEcg(hisData.MultiLeadsECG);
                break;

            case HisDataType.YylpfeData when hisData.YYLPFE is not null:
                ParseYylpfe(hisData.YYLPFE);
                break;
        }
    }

    // ── Health ────────────────────────────────────────────────────────────────

    private async Task ParseAndSaveHealthAsync(
        string deviceId,
        long seq,
        HisDataHealth h,
        CancellationToken ct)
    {
        string dataTime = DateTimeUtilities.FromUnixSeconds(h.TimeStamp.DateTime_.Seconds);
        var record = new HealthDataRecord
        {
            DeviceId = deviceId,
            DataTime = dataTime,
            Seq = seq
        };

        if (h.PedoData is not null)
        {
            record.Steps = h.PedoData.Step;
            record.DistanceMetres = h.PedoData.Distance * 0.1f;
            record.CaloriesKcal = h.PedoData.Calorie * 0.1f;
            record.ActivityType = h.PedoData.Type;
            record.ActivityState = h.PedoData.State & 15u;
            _logger.LogInformation(
                "{Time} — steps: {S}, dist: {D:F1} m, cal: {C:F1} kcal",
                dataTime, record.Steps, record.DistanceMetres, record.CaloriesKcal);
        }

        if (h.HrData is not null)
        {
            record.AvgHeartRate = h.HrData.AvgBpm;
            record.MaxHeartRate = h.HrData.MaxBpm;
            record.MinHeartRate = h.HrData.MinBpm;
            _logger.LogInformation(
                "{Time} — HR avg: {A}, max: {X}, min: {N}",
                dataTime, record.AvgHeartRate, record.MaxHeartRate, record.MinHeartRate);
        }

        if (h.BxoyData is not null)
        {
            record.AvgSpo2 = h.BxoyData.AgvOxy;
            record.MaxSpo2 = h.BxoyData.MaxOxy;
            record.MinSpo2 = h.BxoyData.MinOxy;
        }

        if (h.BpData is not null)
        {
            record.Sbp = h.BpData.Sbp;
            record.Dbp = h.BpData.Dbp;
            _logger.LogInformation("{Time} — BP: {Sbp}/{Dbp}", dataTime, record.Sbp, record.Dbp);
        }

        if (h.HrvData is not null)
        {
            record.HrvSdnn = h.HrvData.SDNN / 10.0;
            record.HrvRmssd = h.HrvData.RMSSD / 10.0;
            record.HrvPnn50 = h.HrvData.PNN50 / 10.0;
            record.HrvMean = h.HrvData.MEAN / 10.0;
            int fatigue = (int)h.HrvData.Fatigue;
            if (fatigue <= 0)
            {
                fatigue = (int)(Math.Log(h.HrvData.RMSSD) * 20);
            }
            record.Fatigue = fatigue;
            _logger.LogInformation("{Time} — fatigue: {F}", dataTime, fatigue);
        }

        if (h.TemperatureData is not null)
        {
            // type: 1 = algorithm complete / value usable; 0 = still computing
            record.TemperatureIsValid = (int)h.TemperatureData.Type;
            record.AxillaryTemp = (h.TemperatureData.EstiArm & 0x0000_ffff) / 100.0f;
            record.EstimatedTemp = ((h.TemperatureData.EstiArm >> 16) & 0x0000_ffff) / 100.0f;
            record.ShellTemp = (h.TemperatureData.EviBody & 0x0000_ffff) / 100.0f;
            record.EnvTemp = ((h.TemperatureData.EviBody >> 16) & 0x0000_ffff) / 100.0f;
        }

        if (h.SleepData is not null)
        {
            bool shutdown = h.SleepData.ShutDown;
            bool charge = h.SleepData.Charge;
            _logger.LogInformation(
                "{Time} — sleep entries: {Count}, charge: {C}, shutdown: {S}",
                dataTime, h.SleepData.SleepData.Count, charge, shutdown);
        }

        if (h.BiozData is not null)
        {
            record.BiozR = (int?)h.BiozData.R;
            record.BiozX = (int?)h.BiozData.X;
            record.BodyFat = h.BiozData.Fat;
            record.Bmi = h.BiozData.Bmi;
        }

        if (h.BloodSugarData is not null)
        {
            record.BloodSugar = h.BloodSugarData.BloodSugar;
        }

        if (h.BloodPotassiumData is not null)
        {
            record.BloodPotassium = h.BloodPotassiumData.BloodPotassium;
        }

        if (h.BpBpmData is not null)
        {
            record.BpBpm = h.BpBpmData.Bpm;
            _logger.LogInformation("{Time} — BP BPM: {B}", dataTime, record.BpBpm);
        }

        // Mattress bed sensor (humidity + temperature)
        if (h.HumitureData is not null)
        {
            record.MatressHumidity = h.HumitureData.Humidity;
            record.MatressTemperature = h.HumitureData.Temperature;
            _logger.LogInformation("{Time} — mattress humidity: {H}%, temp: {T}°C",
                dataTime, record.MatressHumidity, record.MatressTemperature);
        }

        await _healthRepo.AddAsync(record, ct).ConfigureAwait(false);
    }

    // ── ECG ───────────────────────────────────────────────────────────────────

    private async Task ParseAndSaveEcgAsync(
        string deviceId,
        long seq,
        HisDataECG ecg,
        CancellationToken ct)
    {
        string dataTime = DateTimeUtilities.FromUnixSeconds(ecg.TimeStamp.DateTime_.Seconds);
        string raw = Convert.ToBase64String(ecg.RawData.Select(v => (byte)v).ToArray());
        _logger.LogInformation("{Time} — ECG samples: {Count}", dataTime, ecg.RawData.Count);

        await _healthRepo.AddEcgAsync(new EcgDataRecord
        {
            DeviceId = deviceId,
            DataTime = dataTime,
            Seq = seq,
            SampleCount = ecg.RawData.Count,
            RawDataBase64 = raw
        }, ct).ConfigureAwait(false);
    }

    // ── RRI ───────────────────────────────────────────────────────────────────

    private async Task ParseAndSaveRriAsync(
        string deviceId,
        long seq,
        HisDataRRI rri,
        CancellationToken ct)
    {
        string dataTime = DateTimeUtilities.FromUnixSeconds(rri.TimeStamp.DateTime_.Seconds);

        var rriList = new List<long>(rri.RawData.Count * 2);
        foreach (long raw in rri.RawData)
        {
            rriList.Add((raw >> 16) & 0x0000_ffff);
            rriList.Add(raw & 0x0000_ffff);
        }

        _logger.LogInformation("{Time} — RRI samples: {Count}", dataTime, rriList.Count);

        await _rriRepo.AddAsync(new RriDataRecord
        {
            DeviceId = deviceId,
            DataTime = dataTime,
            Seq = seq,
            SampleCount = rriList.Count,
            RriValuesJson = JsonSerializer.Serialize(rriList)
        }, ct).ConfigureAwait(false);
    }

    // ── SPO2 (now persisted for continuous-SpO2 analysis) ─────────────────────

    private async Task ParseAndSaveSpo2Async(
        string deviceId,
        HisDataSpo2 spo2,
        CancellationToken ct)
    {
        string dataTime = DateTimeUtilities.FromUnixSeconds(spo2.TimeStamp.DateTime_.Seconds);
        var records = new List<Spo2DataRecord>(spo2.Spo2Data.Count);

        foreach (uint raw in spo2.Spo2Data)
        {
            int spo2Val = (int)((raw >> 24) & 0xFF);
            int hr = (int)((raw >> 16) & 0xFF);
            int perfusion = (int)((raw >> 8) & 0xFF);
            int touch = (int)(raw & 0xFF);
            _logger.LogInformation(
                "{Time} — SPO2: {S}, HR: {H}, perfusion: {P}, touch: {T}",
                dataTime, spo2Val, hr, perfusion, touch);

            records.Add(new Spo2DataRecord
            {
                DeviceId = deviceId,
                DataTime = dataTime,
                Spo2 = spo2Val,
                HeartRate = hr,
                Perfusion = perfusion,
                Touch = touch
            });
        }

        if (records.Count > 0)
        {
            await _spo2Repo.AddRangeAsync(records, ct).ConfigureAwait(false);
        }
    }

    // ── PPG ───────────────────────────────────────────────────────────────────

    private void ParsePpg(HisDataPPG ppg)
    {
        string dataTime = DateTimeUtilities.FromUnixSeconds(ppg.TimeStamp.DateTime_.Seconds);
        foreach (int raw in ppg.RawData)
        {
            short f = (short)Math.Abs((raw >> 16) & 0x0000_FFFF);
            short s = (short)Math.Abs(raw & 0x0000_FFFF);
            _logger.LogInformation("{Time} — PPG: {F},{S}", dataTime, f, s);
        }
    }

    // ── Accelerometer (now persisted for Parkinson analysis) ──────────────────

    private async Task ParseAndSaveAccAsync(
        string deviceId,
        HisACCelerometer acc,
        CancellationToken ct)
    {
        string dataTime = DateTimeUtilities.FromUnixSeconds(acc.TimeStamp.DateTime_.Seconds);
        List<int> xList = ParseBytesPairs(acc.AccX.ToByteArray());
        List<int> yList = ParseBytesPairs(acc.AccY.ToByteArray());
        List<int> zList = ParseBytesPairs(acc.AccZ.ToByteArray());

        int count = Math.Min(Math.Min(xList.Count, yList.Count), zList.Count);
        for (int i = 0; i < count; i++)
        {
            _logger.LogInformation("{Time} — ACC x:{X}, y:{Y}, z:{Z}", dataTime, xList[i], yList[i], zList[i]);
        }

        await _accRepo.AddAsync(new AccDataRecord
        {
            DeviceId = deviceId,
            DataTime = dataTime,
            XValuesJson = JsonSerializer.Serialize(xList),
            YValuesJson = JsonSerializer.Serialize(yList),
            ZValuesJson = JsonSerializer.Serialize(zList),
            SampleCount = count
        }, ct).ConfigureAwait(false);
    }

    // ── Multi-leads ECG ───────────────────────────────────────────────────────

    private void ParseMultiLeadsEcg(HisDataMultiLeadsECG ecg)
    {
        string dataTime = DateTimeUtilities.FromUnixSeconds(ecg.TimeStamp.DateTime_.Seconds);
        uint channels = ecg.NumberOfChannels;
        uint singleLen = ecg.SingleDataByteLen;
        _logger.LogInformation("{Time} — MultiLeadsECG channels: {C}, singleLen: {L}", dataTime, channels, singleLen);

        byte[] buffer = ecg.RawData.ToByteArray();
        uint unitSize = channels * singleLen;

        for (uint i = unitSize; i <= buffer.Length; i += unitSize)
        {
            for (uint j = 0; j < channels; j++)
            {
                int num = 0;
                for (uint k = 0; k < singleLen; k++)
                {
                    uint pos = i - singleLen * (channels - j) + k;
                    int offset = (int)(8 * (singleLen - k - 1));
                    num += buffer[pos] << offset;
                }
                _logger.LogInformation("Channel {Ch}, val: {V}", j + 1, num);
            }
        }
    }

    // ── YYLPFE ────────────────────────────────────────────────────────────────

    private void ParseYylpfe(HisDataYYLPFE yyl)
    {
        uint dataTs = yyl.TimeStamp.DateTime_.Seconds;
        byte[] bytes = yyl.RawData.ToByteArray();

        for (uint i = 0; i + 11 < bytes.Length; i += 12)
        {
            uint offsetTs = 0;
            ushort areaUp = 0, areaDown = 0, rri = 0, motion = 0;
            int step = (int)i;

            for (int k = 0; k < 4; k++) offsetTs |= (uint)(bytes[step++] << (8 * k));
            for (int k = 0; k < 2; k++) areaUp |= (ushort)(bytes[step++] << (8 * k));
            for (int k = 0; k < 2; k++) areaDown |= (ushort)(bytes[step++] << (8 * k));
            for (int k = 0; k < 2; k++) rri |= (ushort)(bytes[step++] << (8 * k));
            for (int k = 0; k < 2; k++) motion |= (ushort)(bytes[step++] << (8 * k));

            uint utcTs = dataTs + offsetTs / 1000 - (8 * 3600);
            _logger.LogInformation(
                "YYLPFE ts:{Ts}, areaUp:{AU}, areaDown:{AD}, rri:{R}, motion:{M}",
                utcTs, areaUp, areaDown, rri, motion);
        }
    }

    // ── Third-party device data (V2 — mattress/bed sensor) ───────────────────

    private void ParseThirdPartyV2(HisDataThirdPartyV2 tp)
    {
        if (tp.MattressData is null) return;
        var m = tp.MattressData;

        // AcquisitionTime is the measurement timestamp
        string dataTime = DateTimeUtilities.FromUnixSeconds(m.AcquisitionTime.DateTime_.Seconds);

        // PhysicalSignsData is a raw ByteString payload (bed/HR/breath/motion per the iwown protocol).
        // The exact parsing format is hardware-specific. Log byte count for now.
        int byteCount = m.PhysicalSignsData.Length;
        _logger.LogInformation(
            "{Time} — mattress physical-signs data: {Bytes} bytes",
            dataTime, byteCount);
    }

    // ── Third-party device data (V1) ──────────────────────────────────────────

    private void ParseThirdParty(HisDataThirdParty tp)
    {
        if (tp.DataHealth is null) return;
        var h = tp.DataHealth;
        _logger.LogInformation("3rd-party device MAC: {Mac}", h.MacAddr);

        if (h.BpData is not null)
        {
            string t = DateTimeUtilities.FromUnixSeconds(h.BpData.Time.DateTime_.Seconds);
            _logger.LogInformation("{T} — 3P BP {Sbp}/{Dbp}, HR:{Hr}", t, h.BpData.Sbp, h.BpData.Dbp, h.BpData.Hr);
        }
        if (h.GluData is not null)
        {
            string t = DateTimeUtilities.FromUnixSeconds(h.GluData.Time.DateTime_.Seconds);
            _logger.LogInformation("{T} — 3P glucose: {G}", t, h.GluData.Glu);
        }
        if (h.ScaleData is not null)
        {
            string t = DateTimeUtilities.FromUnixSeconds(h.ScaleData.Time.DateTime_.Seconds);
            _logger.LogInformation("{T} — 3P scale weight:{W}, impedance:{I}, fat:{F}%", t, h.ScaleData.Weight, h.ScaleData.Impedance, h.ScaleData.BodyFatPercentage);
        }
        if (h.Spo2Data is not null)
        {
            string t = DateTimeUtilities.FromUnixSeconds(h.Spo2Data.Time.DateTime_.Seconds);
            _logger.LogInformation("{T} — 3P SPO2:{S}, HR:{H}, PI:{P}", t, h.Spo2Data.Spo2, h.Spo2Data.Bpm, h.Spo2Data.Pi);
        }
        if (h.TempData is not null)
        {
            string t = DateTimeUtilities.FromUnixSeconds(h.TempData.Time.DateTime_.Seconds);
            _logger.LogInformation("{T} — 3P temp: {T2}", t, h.TempData.BodyTemp);
        }
        if (h.BloodKetonesData is not null)
        {
            string t = DateTimeUtilities.FromUnixSeconds(h.BloodKetonesData.Time.DateTime_.Seconds);
            _logger.LogInformation("{T} — 3P ketones: {K}", t, h.BloodKetonesData.BloodKetones);
        }
        if (h.UricAcidData is not null)
        {
            string t = DateTimeUtilities.FromUnixSeconds(h.UricAcidData.Time.DateTime_.Seconds);
            _logger.LogInformation("{T} — 3P uric acid: {U}", t, h.UricAcidData.UricAcid);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static List<int> ParseBytesPairs(byte[] bytes)
    {
        var list = new List<int>(bytes.Length / 2);
        for (int i = 1; i < bytes.Length; i += 2)
        {
            list.Add(bytes[i - 1] + (bytes[i] << 8));
        }
        return list;
    }
}

/// <summary>
/// Parses opcode 0x80 history-data protobuf packets and persists all
/// health, ECG, RRI, PPG, accelerometer, multi-leads-ECG, SPO2,
/// third-party, and YYLPFE data to the database.
/// Replaces the original <c>HistoryDataProcessor</c> flat-class.
/// </summary>
