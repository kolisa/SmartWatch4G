using Microsoft.Extensions.Logging;

using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Processors;

/// <summary>
/// Handles both ThirdParty V1 (scalar measurements from paired BT devices)
/// and ThirdParty V2 (raw mattress physical-signs payload).
/// </summary>
internal sealed class ThirdPartyDataHandler : IHisDataHandler
{
    private readonly IThirdPartyDataRepository _repo;
    private readonly ILogger<ThirdPartyDataHandler> _logger;

    public ThirdPartyDataHandler(IThirdPartyDataRepository repo, ILogger<ThirdPartyDataHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public bool CanHandle(HisDataType type, HisData hisData)
        => type == HisDataType.ThirdpartyData
           && (hisData.ThirdPartyData is not null || hisData.ThirdPartyDataV2 is not null);

    public async Task HandleAsync(string deviceId, long seq, HisData hisData, CancellationToken ct)
    {
        if (hisData.ThirdPartyDataV2 is not null)
        {
            HandleV2(hisData.ThirdPartyDataV2);
            return;
        }

        await HandleV1Async(deviceId, hisData.ThirdPartyData!, ct).ConfigureAwait(false);
    }

    // ── V1 ────────────────────────────────────────────────────────────────────

    private async Task HandleV1Async(string deviceId, HisDataThirdParty tp, CancellationToken ct)
    {
        if (tp.DataHealth is null) return;
        var h = tp.DataHealth;
        _logger.LogDebug("3rd-party device MAC: {Mac}", h.MacAddr);

        string dataTime = string.Empty;
        if (h.BpData is not null) dataTime = DateTimeUtilities.FromUnixSeconds(h.BpData.Time.DateTime_.Seconds);
        else if (h.GluData is not null) dataTime = DateTimeUtilities.FromUnixSeconds(h.GluData.Time.DateTime_.Seconds);
        else if (h.ScaleData is not null) dataTime = DateTimeUtilities.FromUnixSeconds(h.ScaleData.Time.DateTime_.Seconds);
        else if (h.Spo2Data is not null) dataTime = DateTimeUtilities.FromUnixSeconds(h.Spo2Data.Time.DateTime_.Seconds);
        else if (h.TempData is not null) dataTime = DateTimeUtilities.FromUnixSeconds(h.TempData.Time.DateTime_.Seconds);
        else if (h.BloodKetonesData is not null) dataTime = DateTimeUtilities.FromUnixSeconds(h.BloodKetonesData.Time.DateTime_.Seconds);
        else if (h.UricAcidData is not null) dataTime = DateTimeUtilities.FromUnixSeconds(h.UricAcidData.Time.DateTime_.Seconds);

        if (string.IsNullOrEmpty(dataTime)) return;

        var record = new ThirdPartyDataRecord { DeviceId = deviceId, MacAddr = h.MacAddr, DataTime = dataTime };

        if (h.BpData is not null)
        {
            record.BpSbp = (int)h.BpData.Sbp;
            record.BpDbp = (int)h.BpData.Dbp;
            record.BpHr = (int)h.BpData.Hr;
            record.BpPulse = (int)h.BpData.Pulse;
            _logger.LogDebug("{T} — 3P BP {Sbp}/{Dbp}, HR:{Hr}", dataTime, record.BpSbp, record.BpDbp, record.BpHr);
        }

        if (h.GluData is not null)
        {
            record.BloodGlucose = h.GluData.Glu;
            _logger.LogDebug("{T} — 3P glucose: {G}", dataTime, record.BloodGlucose);
        }

        if (h.ScaleData is not null)
        {
            record.ScaleWeight = h.ScaleData.Weight;
            record.ScaleImpedance = h.ScaleData.Impedance;
            record.ScaleBodyFatPercentage = h.ScaleData.BodyFatPercentage;
            _logger.LogDebug("{T} — 3P scale weight:{W}, impedance:{I}, fat:{F}%",
                dataTime, record.ScaleWeight, record.ScaleImpedance, record.ScaleBodyFatPercentage);
        }

        if (h.Spo2Data is not null)
        {
            record.OximeterSpo2 = (int)h.Spo2Data.Spo2;
            record.OximeterHr = (int)h.Spo2Data.Bpm;
            record.OximeterPi = h.Spo2Data.Pi;
            _logger.LogDebug("{T} — 3P SPO2:{S}, HR:{H}, PI:{P}",
                dataTime, record.OximeterSpo2, record.OximeterHr, record.OximeterPi);
        }

        if (h.TempData is not null)
        {
            record.BodyTemp = h.TempData.BodyTemp;
            _logger.LogDebug("{T} — 3P temp: {T2}", dataTime, record.BodyTemp);
        }

        if (h.BloodKetonesData is not null)
        {
            record.BloodKetones = h.BloodKetonesData.BloodKetones;
            _logger.LogDebug("{T} — 3P ketones: {K}", dataTime, record.BloodKetones);
        }

        if (h.UricAcidData is not null)
        {
            record.UricAcid = h.UricAcidData.UricAcid;
            _logger.LogDebug("{T} — 3P uric acid: {U}", dataTime, record.UricAcid);
        }

        await _repo.AddAsync(record, ct).ConfigureAwait(false);
    }

    // ── V2 (mattress — raw payload, no structured decode yet) ────────────────

    private void HandleV2(HisDataThirdPartyV2 tp)
    {
        if (tp.MattressData is null) return;
        string dataTime = DateTimeUtilities.FromUnixSeconds(tp.MattressData.AcquisitionTime.DateTime_.Seconds);
        _logger.LogDebug("{Time} — mattress physical-signs data: {Bytes} bytes",
            dataTime, tp.MattressData.PhysicalSignsData.Length);
    }
}
