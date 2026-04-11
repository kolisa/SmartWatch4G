using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Services;

public sealed class MultiLeadsEcgQueryService : IMultiLeadsEcgQueryService
{
    private readonly IMultiLeadsEcgRepository _repo;
    private readonly IDateTimeService _dt;

    public MultiLeadsEcgQueryService(IMultiLeadsEcgRepository repo, IDateTimeService dt)
    {
        _repo = repo;
        _dt = dt;
    }

    public async Task<IReadOnlyList<MultiLeadsEcgDto>> GetByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = _dt.TryGetTimeZone(tz);
        var records = await _repo.GetByDeviceAndDateAsync(deviceId, date, ct).ConfigureAwait(false);

        return records.Select(r => new MultiLeadsEcgDto
        {
            DeviceId = r.DeviceId ?? string.Empty,
            DataTime = _dt.LocalizeTimestamp(r.DataTime, tzInfo),
            Seq = r.Seq,
            Channels = r.Channels,
            SampleByteLen = r.SampleByteLen,
            RawDataBase64 = r.RawDataBase64
        }).ToList();
    }
}
