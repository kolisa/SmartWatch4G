using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Services;

public sealed class PpgQueryService : IPpgQueryService
{
    private readonly IPpgDataRepository _repo;
    private readonly IDateTimeService _dt;

    public PpgQueryService(IPpgDataRepository repo, IDateTimeService dt)
    {
        _repo = repo;
        _dt = dt;
    }

    public async Task<IReadOnlyList<PpgReadingDto>> GetByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = _dt.TryGetTimeZone(tz);
        var records = await _repo.GetByDeviceAndDateAsync(deviceId, date, ct).ConfigureAwait(false);

        return records.Select(r => new PpgReadingDto
        {
            DeviceId = r.DeviceId ?? string.Empty,
            DataTime = _dt.LocalizeTimestamp(r.DataTime, tzInfo),
            Seq = r.Seq,
            SampleCount = r.SampleCount,
            RawDataJson = r.RawDataJson
        }).ToList();
    }
}
