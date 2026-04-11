using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Services;

public sealed class ThirdPartyDataQueryService : IThirdPartyDataQueryService
{
    private readonly IThirdPartyDataRepository _repo;
    private readonly IDateTimeService _dt;

    public ThirdPartyDataQueryService(IThirdPartyDataRepository repo, IDateTimeService dt)
    {
        _repo = repo;
        _dt = dt;
    }

    public async Task<IReadOnlyList<ThirdPartyDataDto>> GetByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = _dt.TryGetTimeZone(tz);
        var records = await _repo.GetByDeviceAndDateAsync(deviceId, date, ct).ConfigureAwait(false);

        return records.Select(r => new ThirdPartyDataDto
        {
            DeviceId = r.DeviceId ?? string.Empty,
            MacAddr = r.MacAddr,
            DataTime = _dt.LocalizeTimestamp(r.DataTime, tzInfo),
            BpSbp = r.BpSbp,
            BpDbp = r.BpDbp,
            BpHr = r.BpHr,
            BpPulse = r.BpPulse,
            ScaleWeight = r.ScaleWeight,
            ScaleImpedance = r.ScaleImpedance,
            ScaleBodyFatPercentage = r.ScaleBodyFatPercentage,
            OximeterSpo2 = r.OximeterSpo2,
            OximeterHr = r.OximeterHr,
            OximeterPi = r.OximeterPi,
            BodyTemp = r.BodyTemp,
            BloodGlucose = r.BloodGlucose,
            BloodKetones = r.BloodKetones,
            UricAcid = r.UricAcid
        }).ToList();
    }
}
