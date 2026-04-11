using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Services;

/// <summary>
/// Infrastructure implementation of <see cref="IEcgQueryService"/>.
/// </summary>
public sealed class EcgQueryService : IEcgQueryService
{
    private readonly IEcgDataRepository _ecgRepo;
    private readonly IDateTimeService _dt;

    public EcgQueryService(IEcgDataRepository ecgRepo, IDateTimeService dt)
    {
        _ecgRepo = ecgRepo;
        _dt = dt;
    }

    public async Task<IReadOnlyList<EcgRecordDto>> GetByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = _dt.TryGetTimeZone(tz);
        var records = await _ecgRepo.GetByDeviceAndDateAsync(deviceId, date, ct)
            .ConfigureAwait(false);

        return records.Select(r => new EcgRecordDto
        {
            DeviceId = r.DeviceId ?? string.Empty,
            DataTime = _dt.LocalizeTimestamp(r.DataTime, tzInfo),
            Seq = r.Seq,
            SampleCount = r.SampleCount,
            RawDataBase64 = r.RawDataBase64
        }).ToList();
    }
}
