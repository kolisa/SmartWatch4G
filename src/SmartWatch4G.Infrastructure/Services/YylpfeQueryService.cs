using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Services;

public sealed class YylpfeQueryService : IYylpfeQueryService
{
    private readonly IYylpfeRepository _repo;
    private readonly IDateTimeService _dt;

    public YylpfeQueryService(IYylpfeRepository repo, IDateTimeService dt)
    {
        _repo = repo;
        _dt = dt;
    }

    public async Task<IReadOnlyList<YylpfeReadingDto>> GetByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = _dt.TryGetTimeZone(tz);
        var records = await _repo.GetByDeviceAndDateAsync(deviceId, date, ct).ConfigureAwait(false);

        return records.Select(r => new YylpfeReadingDto
        {
            DeviceId = r.DeviceId ?? string.Empty,
            DataTime = _dt.LocalizeTimestamp(r.DataTime, tzInfo),
            Seq = r.Seq,
            AreaUp = r.AreaUp,
            AreaDown = r.AreaDown,
            Rri = r.Rri,
            Motion = r.Motion
        }).ToList();
    }
}
