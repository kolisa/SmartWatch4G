using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Services;

/// <summary>
/// Infrastructure implementation of <see cref="IRriQueryService"/>.
/// </summary>
public sealed class RriQueryService : IRriQueryService
{
    private readonly IRriDataRepository _rriRepo;
    private readonly IDateTimeService _dt;

    public RriQueryService(IRriDataRepository rriRepo, IDateTimeService dt)
    {
        _rriRepo = rriRepo;
        _dt = dt;
    }

    public async Task<IReadOnlyList<RriReadingDto>> GetByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = _dt.TryGetTimeZone(tz);
        var records = await _rriRepo.GetByDeviceAndDateAsync(deviceId, date, ct)
            .ConfigureAwait(false);

        return records.Select(r => new RriReadingDto
        {
            DeviceId = r.DeviceId ?? string.Empty,
            DataTime = _dt.LocalizeTimestamp(r.DataTime, tzInfo),
            Seq = r.Seq,
            SampleCount = r.SampleCount,
            RriValuesJson = r.RriValuesJson
        }).ToList();
    }
}
